using System;
using System.Runtime.InteropServices;

namespace Kztek.VideoPlayer.Avalonia.PInvoke
{
    [StructLayout(LayoutKind.Sequential)]
    public struct AVHWFramesContext
    {
        public IntPtr av_class;        // const AVClass*
        public IntPtr device_ref;      // AVBufferRef*
        public IntPtr device_ctx;      // AVHWDeviceContext*

        public AVPixelFormat format;   // GPU pixel format (e.g. AV_PIX_FMT_D3D11)
        public AVPixelFormat sw_format;// Software format (e.g. AV_PIX_FMT_NV12)
        public int width;
        public int height;
        public int initial_pool_size;

        // Bỏ qua các trường khác nếu không dùng
    }

}
