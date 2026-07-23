using System.Runtime.InteropServices;

namespace Kztek.VideoPlayer.Avalonia.PInvoke
{
    /// <summary>
    /// Ánh xạ của struct AVCodecParameters.
    /// Chứa các thông tin về codec của một luồng.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct AVCodecParameters
    {
        public AVMediaType codec_type;
        public AVCodecID codec_id;
        public uint codec_tag;
        public nint extradata;
        public int extradata_size;
        public int format;
        public long bit_rate;
        public int bits_per_coded_sample;
        public int bits_per_raw_sample;
        public int profile;
        public int level;
        public int width;
        public int height;
        // ... các trường khác
    }
}
