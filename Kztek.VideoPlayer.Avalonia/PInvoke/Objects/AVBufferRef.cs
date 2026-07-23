using System;
using System.Runtime.InteropServices;

namespace Kztek.VideoPlayer.Avalonia.PInvoke
{
    [StructLayout(LayoutKind.Sequential)]
    public struct AVBufferRef
    {
        public IntPtr buffer;     // AVBuffer *
        public IntPtr data;       // void * (đây là con trỏ đến AVHWFramesContext, AVCodecContext, ...)
        public int size;          // kích thước vùng nhớ
    }

}
