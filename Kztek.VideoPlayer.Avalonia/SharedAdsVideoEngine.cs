using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Kztek.VideoPlayer.Avalonia.Decoding;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Kztek.VideoPlayer.Avalonia;

/// <summary>
/// Service singleton quản lý 1 luồng decode video quảng cáo duy nhất cho toàn bộ Kiosk.
/// Tất cả <see cref="AdsVideoPlayer"/> ở các màn hình phát CÙNG MỘT DÒNG VIDEO liên tục (seamless),
/// không bị khởi tạo/reset lại khi chuyển màn hình.
/// </summary>
public sealed class SharedAdsVideoEngine
{
    private static readonly Lazy<SharedAdsVideoEngine> _instance = new(() => new SharedAdsVideoEngine());
    public static SharedAdsVideoEngine Instance => _instance.Value;

    private readonly object _lock = new();
    private string? _currentSource;
    private CancellationTokenSource? _cts;
    private VideoStreamDecoderLoop? _decoder;
    private WriteableBitmap? _currentFrame;
    private Task? _decodeTask;

    /// <summary>Event raised mỗi khi có frame video mới trên UI thread.</summary>
    public event EventHandler<WriteableBitmap>? FrameAvailable;

    /// <summary>Frame video hiện tại (an toàn đọc từ UI thread).</summary>
    public WriteableBitmap? CurrentFrame => Volatile.Read(ref _currentFrame);

    /// <summary>Đường dẫn file video hiện tại đang phát.</summary>
    public string? Source => _currentSource;

    /// <summary>True nếu engine đang phát video.</summary>
    public bool IsPlaying => _decoder != null;

    private SharedAdsVideoEngine() { }

    /// <summary>
    /// Đặt file video cần phát. Nếu file đã và đang phát, giữ nguyên dòng phát không restart.
    /// </summary>
    public void SetSource(string? source, bool isLooping = true)
    {
        lock (_lock)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                StopInternal();
                return;
            }

            // Nếu đang phát đúng file này và decoder còn sống, không restart
            if (_currentSource == source && _decoder != null && _cts != null && !_cts.IsCancellationRequested)
            {
                return;
            }

            StopInternal();
            _currentSource = source;

            var cts = new CancellationTokenSource();
            _cts = cts;

            var decoder = new VideoStreamDecoderLoop(source, isLooping);
            if (!decoder.Open())
            {
                Debug.WriteLine($"[SharedAdsVideoEngine] Cannot open video source: {source}");
                decoder.Dispose();
                return;
            }

            _decoder = decoder;
            var token = cts.Token;

            _decodeTask = Task.Run(async () =>
            {
                int srcW = decoder.SourceWidth > 0 ? decoder.SourceWidth : 1280;
                int srcH = decoder.SourceHeight > 0 ? decoder.SourceHeight : 720;

                // Task 1: Decode loop (chạy trên bg thread pool riêng, không chặn Task.Run chính)
                Task decodeLoop = Task.Run(() => decoder.DecodeLoopAsync(() => srcW, () => srcH, token), token);

                // Task 2: Consume FrameChannel -> update CurrentFrame trên UI thread
                Task consumeLoop = Task.Run(async () =>
                {
                    try
                    {
                        await foreach (var bmp in decoder.FrameChannel.Reader.ReadAllAsync(token))
                        {
                            // Swap & return frame cũ TRÊN UI THREAD để tránh race condition với Render()
                            Dispatcher.UIThread.Post(() =>
                            {
                                var old = Interlocked.Exchange(ref _currentFrame, bmp);
                                if (old != null)
                                {
                                    AvaloniaFramePool.Return(old);
                                }

                                FrameAvailable?.Invoke(this, bmp);
                            });
                        }
                    }
                    catch (OperationCanceledException) { }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[SharedAdsVideoEngine] Consume loop error: {ex.Message}");
                    }
                }, token);

                try
                {
                    await Task.WhenAll(decodeLoop, consumeLoop).ConfigureAwait(false);
                }
                catch { }
            }, token);
        }
    }

    /// <summary>Dừng hoàn toàn decode video engine.</summary>
    public void Stop()
    {
        lock (_lock)
        {
            StopInternal();
        }
    }

    private void StopInternal()
    {
        _currentSource = null;
        _cts?.Cancel();
        _cts = null;

        var d = Interlocked.Exchange(ref _decoder, null);
        d?.Dispose();

        var old = Interlocked.Exchange(ref _currentFrame, null);
        if (old != null)
        {
            Dispatcher.UIThread.Post(() => AvaloniaFramePool.Return(old));
        }
    }
}
