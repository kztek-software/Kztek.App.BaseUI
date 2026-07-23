// Extract từ Kztek.Cameras.Avalonia/Players/FFMPEG/UserControls/VideoStreamDecoderIntptr.cs
// Loại bỏ: AI/LPR (ReadPL), motion detection, tracking/counting, recording, OpenVINO, OpenCvSharp,
//          AiChannel, MdChannel, MotionFrame, MdDetection, VirtualLoopType, EmObjectBoxStatus.
// Giữ lại: decode pipeline, FrameChannel (WriteableBitmap), sws_scale (BGRA), AvaloniaFramePool.
// Thêm mới: loop logic (AVERROR_EOF → av_seek_frame + avcodec_flush_buffers, gap ~0ms).
//
// Namespace: Kztek.VideoPlayer.Avalonia.Decoding (tách hoàn toàn khỏi camera stack)
using Avalonia.Media.Imaging;
using Kztek.VideoPlayer.Avalonia.PInvoke;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Channels;

namespace Kztek.VideoPlayer.Avalonia.Decoding;

/// <summary>
/// Decode video file local (không RTSP) bằng FFmpeg P/Invoke — phát lại theo loop hoặc 1 lần.
/// Chỉ xử lý track video đầu tiên (không audio). Frame được ghi vào <see cref="FrameChannel"/>
/// dưới dạng <see cref="WriteableBitmap"/> (Bgra8888) từ <see cref="AvaloniaFramePool"/>.
/// </summary>
public sealed class VideoStreamDecoderLoop : IDisposable
{
    // ---- FFmpeg native context handles ----
    private IntPtr _fmtCtx;
    private IntPtr _pCodecContext;
    private IntPtr _pFrame;
    private IntPtr _pPacket;
    private int _streamIndex;
    private AVRational _timeBase;

    // ---- Playback state ----
    private long _playbackStartTick;
    private long _streamStartPts = long.MinValue;
    private bool _isLooping;
    private readonly string _filePath;

    // ---- Frame output ----
    private readonly BoundedChannelOptions _channelOptions = new(3)
    {
        FullMode = BoundedChannelFullMode.DropOldest
    };
    public Channel<WriteableBitmap> FrameChannel { get; }

    // ---- SWS cache ----
    private sealed class SwsCacheState
    {
        public IntPtr Ctx = IntPtr.Zero;
        public int SrcW, SrcH, DstW, DstH, SrcFmt;
    }
    private readonly SwsCacheState _swsCache = new();
    private readonly object _swsLock = new();

    // FFmpeg error codes
    private const int AVERROR_EAGAIN = -11;
    private const int AVERROR_EOF = -541478725;  // AVERROR(EOF) = -('E'|('O'<<8)|('F'<<16)|(' '<<24))
    private const int AVSEEK_FLAG_BACKWARD = 1;

    private readonly object _closeLock = new();
    private volatile bool _isClosed;

    public int SourceWidth { get; private set; }
    public int SourceHeight { get; private set; }

    public VideoStreamDecoderLoop(string filePath, bool isLooping = true)
    {
        _filePath = filePath;
        _isLooping = isLooping;
        FrameChannel = Channel.CreateBounded<WriteableBitmap>(_channelOptions);
    }

    /// <summary>
    /// Mở file và khởi tạo codec context. Trả về true nếu thành công.
    /// </summary>
    public bool Open()
    {
        lock (_closeLock)
        {
            IntPtr options = IntPtr.Zero;
            try
            {
                // Mở file local (không RTSP)
                if (FFMPEG.avformat_open_input(ref _fmtCtx, _filePath, IntPtr.Zero, ref options) < 0)
                {
                    Debug.WriteLine($"[VideoStreamDecoderLoop.Open] avformat_open_input failed: {_filePath}");
                    return false;
                }

                if (FFMPEG.avformat_find_stream_info(_fmtCtx, IntPtr.Zero) < 0)
                {
                    Debug.WriteLine("[VideoStreamDecoderLoop.Open] avformat_find_stream_info failed");
                    return false;
                }

                var fmtCtxStruct = Marshal.PtrToStructure<AVFormatContext>(_fmtCtx);
                IntPtr codecCtx = FindAndOpenVideoCodec(fmtCtxStruct);
                if (codecCtx == IntPtr.Zero)
                {
                    Debug.WriteLine("[VideoStreamDecoderLoop.Open] No video stream found");
                    return false;
                }

                // Đọc timebase từ stream
                IntPtr streamPtr = Marshal.ReadIntPtr(fmtCtxStruct.streams, _streamIndex * IntPtr.Size);
                var stream = Marshal.PtrToStructure<AVStream>(streamPtr);
                _timeBase = stream.time_base;

                var codecCtxStruct = Marshal.PtrToStructure<AVCodecContext>(codecCtx);
                SourceWidth = codecCtxStruct.width;
                SourceHeight = codecCtxStruct.height;

                _pPacket = FFMPEG.av_packet_alloc();
                _pFrame = FFMPEG.av_frame_alloc();

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VideoStreamDecoderLoop.Open] Exception: {ex.Message}");
                CloseInternal();
                return false;
            }
            finally
            {
                if (options != IntPtr.Zero)
                {
                    FFMPEG.av_dict_free(ref options);
                    options = IntPtr.Zero;
                }
            }
        }
    }

    private IntPtr FindAndOpenVideoCodec(AVFormatContext fmtCtxStruct)
    {
        for (int i = 0; i < fmtCtxStruct.nb_streams; i++)
        {
            IntPtr streamPtr = Marshal.ReadIntPtr(fmtCtxStruct.streams, i * IntPtr.Size);
            if (streamPtr == IntPtr.Zero) continue;

            var stream = Marshal.PtrToStructure<AVStream>(streamPtr);
            var codecpar = Marshal.PtrToStructure<AVCodecParameters>(stream.codecpar);

            if (codecpar.codec_type != AVMediaType.AVMEDIA_TYPE_VIDEO)
                continue;

            IntPtr codec = FFMPEG.avcodec_find_decoder(codecpar.codec_id);
            if (codec == IntPtr.Zero) continue;

            IntPtr codecCtx = FFMPEG.avcodec_alloc_context3(codec);
            _pCodecContext = codecCtx;

            FFMPEG.avcodec_parameters_to_context(codecCtx, stream.codecpar);

            IntPtr temp = IntPtr.Zero;
            if (FFMPEG.avcodec_open2(codecCtx, codec, ref temp) < 0)
            {
                FFMPEG.avcodec_free_context(ref codecCtx);
                _pCodecContext = IntPtr.Zero;
                continue;
            }

            _streamIndex = i;
            return codecCtx;
        }

        return IntPtr.Zero;
    }

    /// <summary>
    /// Loop decode chính — chạy trên background thread cho đến khi token bị hủy.
    /// Với IsLooping=true: khi EOF (rd < 0), seek về 0 + flush_buffers, tiếp tục decode (~0ms gap).
    /// Với IsLooping=false: hoàn thành channel rồi thoát.
    /// </summary>
    public async Task DecodeLoopAsync(Func<int> getWidth, Func<int> getHeight, CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested && !_isClosed)
            {
                int rd;
                lock (_closeLock)
                {
                    if (_isClosed || _fmtCtx == IntPtr.Zero || _pPacket == IntPtr.Zero)
                        break;

                    rd = FFMPEG.av_read_frame(_fmtCtx, _pPacket);
                }

                if (rd < 0)
                {
                    if (_isLooping)
                    {
                        lock (_closeLock)
                        {
                            if (_isClosed || _fmtCtx == IntPtr.Zero || _pCodecContext == IntPtr.Zero)
                                break;

                            FFMPEG.av_seek_frame(_fmtCtx, _streamIndex, 0, AVSEEK_FLAG_BACKWARD);
                            FFMPEG.avcodec_flush_buffers(_pCodecContext);
                        }
                        _streamStartPts = long.MinValue;   // reset ThrottleLocalPlayback baseline
                        _playbackStartTick = 0;
                        await Task.Delay(10, token).ConfigureAwait(false);
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }

                try
                {
                    long pts = long.MinValue;
                    int pktStreamIdx = -1;

                    lock (_closeLock)
                    {
                        if (_isClosed || _pPacket == IntPtr.Zero)
                            break;

                        var pkt = Marshal.PtrToStructure<AVPacket>(_pPacket);
                        pktStreamIdx = pkt.stream_index;
                        pts = pkt.pts;
                    }

                    if (pktStreamIdx != _streamIndex)
                    {
                        lock (_closeLock)
                        {
                            if (!_isClosed && _pPacket != IntPtr.Zero)
                                FFMPEG.av_packet_unref(_pPacket);
                        }
                        continue;
                    }

                    // Throttle ngoài lock để không block UI thread nếu UI gọi CloseInternal
                    ThrottleLocalPlayback(pts);

                    lock (_closeLock)
                    {
                        if (_isClosed || _pCodecContext == IntPtr.Zero || _pFrame == IntPtr.Zero)
                            break;

                        int send = FFMPEG.avcodec_send_packet(_pCodecContext, _pPacket);
                        if (send < 0 && send != AVERROR_EAGAIN)
                            continue;

                        // Tính kích thước frame decode theo layout control hiện tại
                        int ctrlW = getWidth();
                        int ctrlH = getHeight();
                        int dstW = ctrlW >= 32 ? ctrlW : (SourceWidth > 0 ? SourceWidth : 640);
                        int dstH = ctrlH >= 32 ? ctrlH : (SourceHeight > 0 ? SourceHeight : 360);
                        dstW = Math.Max(2, (dstW / 2) * 2);
                        dstH = Math.Max(2, (dstH / 2) * 2);

                        while (!_isClosed)
                        {
                            int r = FFMPEG.avcodec_receive_frame(_pCodecContext, _pFrame);
                            if (r == 0)
                            {
                                var bmp = ConvertFrameToWriteableBitmap(_pFrame, dstW, dstH);
                                if (bmp != null)
                                    WriteToChannelSafely(bmp);

                                FFMPEG.av_frame_unref(_pFrame);
                                continue;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[VideoStreamDecoderLoop] Decode error: {ex.Message}");
                }
                finally
                {
                    lock (_closeLock)
                    {
                        if (!_isClosed && _pPacket != IntPtr.Zero)
                            FFMPEG.av_packet_unref(_pPacket);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal cancellation
        }
        finally
        {
            FrameChannel.Writer.TryComplete();
            CloseInternal();
        }
    }

    /// <summary>
    /// Throttle playback để bám PTS (tránh phát quá nhanh hơn realtime).
    /// Giống pattern từ VideoStreamDecoderIntptr (ThrottleLocalPlayback).
    /// </summary>
    private void ThrottleLocalPlayback(long pts)
    {
        if (pts == long.MinValue || pts < 0) return;
        if (_timeBase.den == 0) return;

        if (_streamStartPts == long.MinValue)
        {
            _streamStartPts = pts;
            _playbackStartTick = Environment.TickCount64;
            return;
        }

        double streamElapsedMs = (pts - _streamStartPts) * 1000.0 * _timeBase.num / _timeBase.den;
        double wallElapsedMs = Environment.TickCount64 - _playbackStartTick;

        int sleepMs = (int)(streamElapsedMs - wallElapsedMs);
        if (sleepMs > 1)
            Thread.Sleep(Math.Min(sleepMs, 500));
    }

    /// <summary>
    /// Convert AVFrame → WriteableBitmap (Bgra8888) qua sws_scale.
    /// Rent từ AvaloniaFramePool — caller gọi AvaloniaFramePool.Return() sau khi hiển thị xong.
    /// </summary>
    private unsafe WriteableBitmap? ConvertFrameToWriteableBitmap(IntPtr framePtr, int dstW, int dstH)
    {
        if (framePtr == IntPtr.Zero) return null;
        var av = Marshal.PtrToStructure<AVFrame>(framePtr);

        if (av.data0 == IntPtr.Zero || av.width <= 0 || av.height <= 0 || dstW <= 0 || dstH <= 0)
            return null;

        const AVPixelFormat DST_FMT = AVPixelFormat.AV_PIX_FMT_BGRA;

        // Rebuild sws context nếu kích thước/format thay đổi
        if (_swsCache.Ctx == IntPtr.Zero ||
            _swsCache.SrcW != av.width || _swsCache.SrcH != av.height ||
            _swsCache.DstW != dstW || _swsCache.DstH != dstH ||
            _swsCache.SrcFmt != (int)av.format)
        {
            _swsCache.Ctx = FFMPEG.sws_getCachedContext(
                _swsCache.Ctx,
                av.width, av.height, (int)(AVPixelFormat)av.format,
                dstW, dstH, (int)DST_FMT,
                SwsFlags.SWS_BILINEAR,
                IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

            _swsCache.SrcW = av.width; _swsCache.SrcH = av.height;
            _swsCache.DstW = dstW; _swsCache.DstH = dstH;
            _swsCache.SrcFmt = (int)av.format;

            if (_swsCache.Ctx == IntPtr.Zero) return null;
        }

        var bmp = AvaloniaFramePool.Rent(dstW, dstH);

        try
        {
            using var fb = bmp.Lock();

            IntPtr* srcData = stackalloc IntPtr[8];
            int* srcStride = stackalloc int[8];
            srcData[0] = av.data0;
            srcData[1] = av.data1;
            srcData[2] = av.data2;
            srcData[3] = av.data3;
            srcData[4] = av.data4;
            srcData[5] = av.data5;
            srcData[6] = av.data6;
            srcData[7] = av.data7;

            srcStride[0] = av.linesize0;
            srcStride[1] = av.linesize1;
            srcStride[2] = av.linesize2;
            srcStride[3] = av.linesize3;
            srcStride[4] = av.linesize4;
            srcStride[5] = av.linesize5;
            srcStride[6] = av.linesize6;
            srcStride[7] = av.linesize7;

            IntPtr* dstData = stackalloc IntPtr[8];
            int* dstStride = stackalloc int[8];
            for (int i = 0; i < 8; i++)
            {
                dstData[i] = IntPtr.Zero;
                dstStride[i] = 0;
            }
            dstData[0] = fb.Address;
            dstStride[0] = fb.RowBytes;

            int scaled = FFMPEG.sws_scale(
                _swsCache.Ctx,
                srcData, srcStride,
                0, av.height,
                dstData, dstStride);

            if (scaled <= 0)
            {
                AvaloniaFramePool.Return(bmp);
                return null;
            }

            return bmp;
        }
        catch
        {
            AvaloniaFramePool.Return(bmp);
            return null;
        }
    }

    private void WriteToChannelSafely(WriteableBitmap bmp)
    {
        if (!FrameChannel.Writer.TryWrite(bmp))
        {
            // Channel full — drop frame, trả về pool
            AvaloniaFramePool.Return(bmp);
        }
    }

    /// <summary>
    /// Đóng file + giải phóng codec context. Không giải phóng FrameChannel (caller đọc hết rồi mới close).
    /// Safe to call multiple times.
    /// </summary>
    private void CloseInternal()
    {
        lock (_closeLock)
        {
            if (_isClosed) return;
            _isClosed = true;

            if (_swsCache.Ctx != IntPtr.Zero)
            {
                FFMPEG.sws_freeContext(_swsCache.Ctx);
                _swsCache.Ctx = IntPtr.Zero;
            }

            if (_pFrame != IntPtr.Zero)
            {
                FFMPEG.av_frame_unref(_pFrame);
                IntPtr p = _pFrame;
                FFMPEG.av_frame_free(ref p);
                _pFrame = IntPtr.Zero;
            }

            if (_pPacket != IntPtr.Zero)
            {
                FFMPEG.av_packet_unref(_pPacket);
                IntPtr p = _pPacket;
                FFMPEG.av_packet_free(ref p);
                _pPacket = IntPtr.Zero;
            }

            if (_pCodecContext != IntPtr.Zero)
            {
                IntPtr p = _pCodecContext;
                FFMPEG.avcodec_free_context(ref p);
                _pCodecContext = IntPtr.Zero;
            }

            if (_fmtCtx != IntPtr.Zero)
            {
                IntPtr p = _fmtCtx;
                FFMPEG.avformat_close_input(ref p);
                _fmtCtx = IntPtr.Zero;
            }
        }
    }

    public void Dispose()
    {
        FrameChannel.Writer.TryComplete();

        // Drain và trả về pool tất cả frame còn lại trong channel
        while (FrameChannel.Reader.TryRead(out var bmp))
            AvaloniaFramePool.Return(bmp);

        CloseInternal();
        GC.SuppressFinalize(this);
    }
}
