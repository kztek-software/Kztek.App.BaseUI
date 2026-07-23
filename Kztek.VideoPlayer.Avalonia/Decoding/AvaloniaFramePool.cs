// C2 fix (RAM leak BUG-004, 2026-07-20): pool cho Avalonia WriteableBitmap — dùng cho luồng hiển thị
// (FrameChannel) thay cho SKBitmapPool/SKBitmap. Lý do: đã chứng minh bằng thực nghiệm (so sánh trực
// tiếp với System.Drawing.Bitmap chạy CÙNG pipeline sws_scale) rằng SKBitmap (SkiaSharp 4.150.1) rò rỉ
// native memory liên tục khi được ghi pixel qua GetPixels()+NotifyPixelsChanged() ở tần suất cao (~30fps),
// trong khi ghi thẳng vào WriteableBitmap.Lock() (cùng cơ chế con trỏ thô như GDI+ LockBits) thì không.
// Cấu trúc pool giữ nguyên pattern SKBitmapPool (ConcurrentDictionary + ConcurrentQueue, MaxItemsPerSize).
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System.Collections.Concurrent;

namespace Kztek.VideoPlayer.Avalonia.Decoding
{
    public static class AvaloniaFramePool
    {
        private static readonly ConcurrentDictionary<(int, int), ConcurrentQueue<WriteableBitmap>> _pool = new();
        private const int MaxItemsPerSize = 15;

        public static WriteableBitmap Rent(int width, int height)
        {
            var key = (width, height);
            if (_pool.TryGetValue(key, out var queue) && queue.TryDequeue(out var bmp))
                return bmp;

            return new WriteableBitmap(
                new PixelSize(width, height),
                new Vector(96, 96),
                PixelFormat.Bgra8888,
                AlphaFormat.Premul);
        }

        public static void Return(WriteableBitmap? bmp)
        {
            if (bmp == null) return;

            var key = (bmp.PixelSize.Width, bmp.PixelSize.Height);
            var queue = _pool.GetOrAdd(key, _ => new ConcurrentQueue<WriteableBitmap>());

            if (queue.Count < MaxItemsPerSize)
            {
                queue.Enqueue(bmp);
            }
            else
            {
                bmp.Dispose();
            }
        }

        /// <summary>
        /// Fix (2026-07-20, loại bỏ SkiaSharp): tạo bản sao WriteableBitmap MỚI (KHÔNG qua pool — caller
        /// giữ lâu để bắn event MotionAnalyzing/AIDetectAnalyzing, không phải vòng đời ngắn hạn của pool).
        /// Caller PHẢI tự Dispose() kết quả sau khi dùng xong.
        /// </summary>
        public static unsafe WriteableBitmap Clone(WriteableBitmap src)
        {
            var result = new WriteableBitmap(src.PixelSize, src.Dpi, src.Format, src.AlphaFormat);
            using var srcFb = src.Lock();
            using var dstFb = result.Lock();
            void* srcPtr = srcFb.Address.ToPointer();
            void* dstPtr = dstFb.Address.ToPointer();
            long byteCount = (long)srcFb.RowBytes * src.PixelSize.Height;
            Buffer.MemoryCopy(srcPtr, dstPtr, byteCount, byteCount);
            return result;
        }
    }
}
