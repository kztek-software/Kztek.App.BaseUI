using System.Runtime.InteropServices;

namespace Kztek.VideoPlayer.Avalonia.PInvoke
{
    [StructLayout(LayoutKind.Sequential)]
    public struct AVClass
    {
        /// <summary>
        /// string
        /// </summary>
        public nint class_name;
        /// <summary>
        /// string
        /// </summary>
        public nint item_name;
        /// <summary>
        /// <see cref="AVOption"/>
        /// </summary>
        public nint option;
        public int version;
        public int log_level_offset_offset;
        public int parent_log_context_offset;
        public AVClassCategory category;
        public nint get_category;
        public nint query_ranges;
        public nint child_next;
        public nint child_class_iterate;
    }
}
