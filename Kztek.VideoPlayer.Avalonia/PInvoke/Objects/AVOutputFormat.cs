using System.Runtime.InteropServices;

namespace Kztek.VideoPlayer.Avalonia.PInvoke
{

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct AVOutputFormat
    {
        /// <summary>
        /// Strinh
        /// </summary>
        public nint name;
        public nint long_name;
        public nint mime_type;
        public nint extensions;
        public AVCodecID audio_codec;
        public AVCodecID video_codec;
        public AVCodecID subtitle_codec;
        public int flags;
        public nint codec_tag;
        public nint priv_class;
    }
}
