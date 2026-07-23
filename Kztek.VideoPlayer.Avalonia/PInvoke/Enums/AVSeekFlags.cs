using System;

namespace Kztek.VideoPlayer.Avalonia.PInvoke
{
    /// <summary>
    /// Các cờ cho việc tìm kiếm (seeking).
    /// </summary>
    [Flags]
    public enum AVSeekFlags
    {
        AVSEEK_FLAG_BACKWARD = 1, ///< seek backward
        AVSEEK_FLAG_BYTE = 2,     ///< seeking based on position in bytes
        AVSEEK_FLAG_ANY = 4,      ///< seek to any frame, even non-keyframes
        AVSEEK_FLAG_FRAME = 8,    ///< seeking based on frame number
    }
}
