using System.Runtime.InteropServices;

namespace Kztek.VideoPlayer.Avalonia.PInvoke
{
    [StructLayout(LayoutKind.Explicit)]
    public struct AVOptionDefaultValue
    {
        [FieldOffset(0)]
        public long i64;
        [FieldOffset(0)]
        public double dbl;
        [FieldOffset(0)]
        public nint str;
        [FieldOffset(0)]
        public AVRational q;
    }
}
