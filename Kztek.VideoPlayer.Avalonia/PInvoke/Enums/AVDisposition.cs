using System;

namespace Kztek.VideoPlayer.Avalonia.PInvoke
{
    /// <summary>
    /// Cách một luồng được sắp đặt trong file media.
    /// </summary>
    [Flags]
    public enum AVDisposition
    {
        AV_DISPOSITION_DEFAULT = 0x1,
        AV_DISPOSITION_DUB = 0x2,
        AV_DISPOSITION_ORIGINAL = 0x4,
        AV_DISPOSITION_COMMENT = 0x8,
        AV_DISPOSITION_LYRICS = 0x10,
        AV_DISPOSITION_KARAOKE = 0x20,
        AV_DISPOSITION_FORCED = 0x40,
        AV_DISPOSITION_HEARING_IMPAIRED = 0x80,
        AV_DISPOSITION_VISUAL_IMPAIRED = 0x100,
        AV_DISPOSITION_CLEAN_EFFECTS = 0x200,
        AV_DISPOSITION_ATTACHED_PIC = 0x400,
        AV_DISPOSITION_TIMED_THUMBNAILS = 0x800,
    }
}
