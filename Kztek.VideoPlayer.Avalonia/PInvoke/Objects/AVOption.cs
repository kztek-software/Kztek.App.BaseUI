using System.Runtime.InteropServices;

namespace Kztek.VideoPlayer.Avalonia.PInvoke
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct AVOption
    {
        public nint name;
        public nint help;
        public int offset;
        public AVOptionType type;
        public AVOptionDefaultValue default_val;
        public double min;
        public double max;
        public int flags;
        public nint unit;
    }
}
