using System.Runtime.InteropServices;

namespace Kztek.VideoPlayer.Avalonia.PInvoke
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct AVStream
    {
        public nint av_class;
        public int index;
        public int id;
        public nint codecpar;
        public nint priv_data;
        public AVRational time_base;
        public long start_time;
        public long duration;
        public long nb_frames;
        public int disposition;
        public AVDiscard discard;
        public AVRational sample_aspect_ratio;
        public nint metadata;
        public AVRational avg_frame_rate;
        public AVPacket attached_pic;
    }
}
