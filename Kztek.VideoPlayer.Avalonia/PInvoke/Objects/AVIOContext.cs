using System.Runtime.InteropServices;

namespace Kztek.VideoPlayer.Avalonia.PInvoke
{
    /// <summary>
    /// Ánh xạ của struct AVIOContext từ C (avio.h).
    /// Đại diện cho một context I/O có bộ đệm (Bytestream IO Context).
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct AVIOContext
    {
        // Đặt các định nghĩa này bên ngoài lớp Form1

        /// <summary>
        /// Delegate cho hàm đọc packet.
        /// </summary>
        /// <param name="opaque">Con trỏ private, được truyền qua từ AVIOContext.</param>
        /// <param name="buf">Buffer để chứa dữ liệu đọc ra.</param>
        /// <param name="buf_size">Kích thước của buffer.</param>
        /// <returns>Số byte đã đọc, hoặc mã lỗi.</returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ReadPacketFunc(nint opaque, byte[] buf, int buf_size);

        /// <summary>
        /// Delegate cho hàm ghi packet.
        /// </summary>
        /// <param name="opaque">Con trỏ private.</param>
        /// <param name="buf">Buffer chứa dữ liệu cần ghi.</param>
        /// <param name="buf_size">Số byte cần ghi.</param>
        /// <returns>Số byte đã ghi, hoặc mã lỗi.</returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int WritePacketFunc(nint opaque, byte[] buf, int buf_size);

        /// <summary>
        /// Delegate cho hàm tìm kiếm (seek).
        /// </summary>
        /// <param name="opaque">Con trỏ private.</param>
        /// <param name="offset">Vị trí offset cần seek tới.</param>
        /// <param name="whence">Điểm gốc để seek (ví dụ: SEEK_SET, SEEK_CUR, SEEK_END).</param>
        /// <returns>Vị trí mới trong stream, hoặc mã lỗi.</returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate long SeekFunc(nint opaque, long offset, int whence);

        /// <summary>
        /// Con trỏ tới AVClass.
        /// </summary>
        public nint av_class;

        /// <summary>
        /// Con trỏ tới đầu của bộ đệm.
        /// </summary>
        public nint buffer;

        /// <summary>
        /// Kích thước của bộ đệm.
        /// </summary>
        public int buffer_size;

        /// <summary>
        /// Con trỏ tới vị trí hiện tại trong bộ đệm.
        /// </summary>
        public nint buf_ptr;

        /// <summary>
        /// Con trỏ tới cuối vùng dữ liệu hợp lệ trong bộ đệm.
        /// </summary>
        public nint buf_end;

        /// <summary>
        /// Con trỏ private được truyền cho các hàm callback (read/write/seek).
        /// </summary>
        public nint opaque;

        /// <summary>
        /// Con trỏ hàm đọc.
        /// </summary>
        public nint read_packet; // ReadPacketFunc

        /// <summary>
        /// Con trỏ hàm ghi.
        /// </summary>
        public nint write_packet; // WritePacketFunc

        /// <summary>
        /// Con trỏ hàm seek.
        /// </summary>
        public nint seek; // SeekFunc

        /// <summary>
        /// Vị trí trong file của đầu bộ đệm.
        /// </summary>
        public long pos;

        /// <summary>
        /// Cờ báo đã tới cuối file (true nếu không thể đọc do lỗi hoặc EOF).
        /// </summary>
        public int eof_reached;

        /// <summary>
        /// Chứa mã lỗi hoặc 0 nếu không có lỗi.
        /// </summary>
        public int error;

        /// <summary>
        /// Cờ báo đang ở chế độ ghi (true nếu mở để ghi).
        /// </summary>
        public int write_flag;

        public int max_packet_size;
        public int min_packet_size;
        public ulong checksum;
        public nint checksum_ptr;
        public nint update_checksum; // Con trỏ hàm
        public nint read_pause;    // Con trỏ hàm
        public nint read_seek;     // Con trỏ hàm

        /// <summary>
        /// Một tập hợp các cờ AVIO_SEEKABLE_* hoặc 0 nếu không thể seek.
        /// </summary>
        public int seekable;

        public int direct;

        public nint protocol_whitelist;   // const char*
        public nint protocol_blacklist;   // const char*
        public nint write_data_type;      // Con trỏ hàm
        public int ignore_boundary_point;
        public nint buf_ptr_max;

        /// <summary>
        /// Thống kê chỉ đọc về số byte đã đọc.
        /// </summary>
        public long bytes_read;

        /// <summary>
        /// Thống kê chỉ đọc về số byte đã ghi.
        /// </summary>
        public long bytes_written;
    }
}
