using System.Runtime.InteropServices;

namespace Kztek.VideoPlayer.Avalonia.PInvoke
{
    [StructLayout(LayoutKind.Sequential)]
    public struct AVFrame
    {
        public nint data0;
        public nint data1;
        public nint data2;
        public nint data3;
        public nint data4;
        public nint data5;
        public nint data6;
        public nint data7;

        public int linesize0;
        public int linesize1;
        public int linesize2;
        public int linesize3;
        public int linesize4;
        public int linesize5;
        public int linesize6;
        public int linesize7;

        public nint extended_data;  // cần có để offset đúng

        public int width;
        public int height;
        public int nb_samples;
        public int format;  // <-- giờ đúng offset
        public int key_frame;
        public int pict_type;
        // ... các trường khác nếu cần
    }
}
