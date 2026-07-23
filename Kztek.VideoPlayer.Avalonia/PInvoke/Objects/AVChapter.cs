using System.Runtime.InteropServices;

namespace Kztek.VideoPlayer.Avalonia.PInvoke
{

    [StructLayout(LayoutKind.Sequential)]
    public struct AVChapter
    {
        /// <summary>
        /// ID duy nhất để xác định chương.
        /// </summary>
        public long id;

        /// <summary>
        /// Cơ sở thời gian mà start/end timestamps được chỉ định.
        /// </summary>
        public AVRational time_base;

        /// <summary>
        /// Thời gian bắt đầu của chương.
        /// </summary>
        public long start;

        /// <summary>
        /// Thời gian kết thúc của chương.
        /// </summary>
        public long end;

        /// <summary>
        /// Con trỏ tới AVDictionary chứa metadata.
        /// </summary>
        public nint metadata;
    }
}
