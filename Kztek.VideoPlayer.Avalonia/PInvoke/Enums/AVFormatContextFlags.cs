using System;

namespace Kztek.VideoPlayer.Avalonia.PInvoke
{
    /// <summary>
    /// Các cờ cho AVFormatContext.flags.
    /// </summary>
    [Flags]
    public enum AVFormatContextFlags
    {
        AVFMT_FLAG_GENPTS = 0x0001,
        AVFMT_FLAG_IGNIDX = 0x0002,
        AVFMT_FLAG_NONBLOCK = 0x0004,
        AVFMT_FLAG_IGNDTS = 0x0008,
        AVFMT_FLAG_NOFILLIN = 0x0010,
        AVFMT_FLAG_NOPARSE = 0x0020,
        AVFMT_FLAG_NOBUFFER = 0x0040,
        AVFMT_FLAG_CUSTOM_IO = 0x0080,
        AVFMT_FLAG_DISCARD_CORRUPT = 0x0100,
        AVFMT_FLAG_FLUSH_PACKETS = 0x0200,
        AVFMT_FLAG_BITEXACT = 0x0400,
        AVFMT_FLAG_SORT_DTS = 0x10000,
        AVFMT_FLAG_FAST_SEEK = 0x80000,
        AVFMT_FLAG_AUTO_BSF = 0x200000,
    }
}
