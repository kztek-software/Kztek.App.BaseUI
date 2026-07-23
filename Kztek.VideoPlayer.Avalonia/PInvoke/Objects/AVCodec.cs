using System.Runtime.InteropServices;

namespace Kztek.VideoPlayer.Avalonia.PInvoke
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct AVCodec
    {
        /// <summary>
        /// Tên của việc triển khai codec.
        /// </summary>
        public nint name;

        /// <summary>
        /// Tên mô tả cho codec.
        /// </summary>
        public nint long_name;

        public AVMediaType type;
        public AVCodecID id;
        public int capabilities;
        public byte max_lowres;
        public nint supported_framerates;
        public nint pix_fmts;
        public nint supported_samplerates;
        public nint sample_fmts;
        public nint priv_class;
        public nint profiles;
        public nint wrapper_name;
        public nint ch_layouts;
    }
}
