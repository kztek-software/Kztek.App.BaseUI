using System.Runtime.InteropServices;

namespace Kztek.VideoPlayer.Avalonia.PInvoke
{
    /// <summary>
    /// Ánh xạ của struct AVPacket từ C.
    /// Cấu trúc này lưu trữ dữ liệu nén. Nó thường được xuất ra bởi các demuxer
    /// và sau đó được chuyển làm đầu vào cho các bộ giải mã, hoặc nhận làm đầu ra từ các bộ mã hóa và
    /// sau đó được chuyển đến các muxer.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct AVPacket
    {
        /// <summary>
        /// Một tham chiếu đến bộ đệm được đếm tham chiếu, nơi dữ liệu của packet được lưu trữ.
        /// Có thể là NULL, khi đó dữ liệu của packet không được đếm tham chiếu.
        /// </summary>
        public nint buf; // AVBufferRef*

        /// <summary>
        /// Dấu thời gian trình chiếu (Presentation timestamp) tính theo đơn vị AVStream->time_base;
        /// thời điểm mà packet đã được giải nén sẽ được trình chiếu cho người dùng.
        /// Có thể là AV_NOPTS_VALUE nếu nó không được lưu trữ trong file.
        /// </summary>
        public long pts;

        /// <summary>
        /// Dấu thời gian giải nén (Decompression timestamp) tính theo đơn vị AVStream->time_base;
        /// thời điểm mà packet được giải nén.
        /// Có thể là AV_NOPTS_VALUE nếu nó không được lưu trữ trong file.
        /// </summary>
        public long dts;

        /// <summary>
        /// Con trỏ tới dữ liệu.
        /// </summary>
        public nint data; // uint8_t*

        /// <summary>
        /// Kích thước của dữ liệu.
        /// </summary>
        public int size;

        /// <summary>
        /// Chỉ số của luồng mà packet này thuộc về.
        /// </summary>
        public int stream_index;

        /// <summary>
        /// Một sự kết hợp của các giá trị AV_PKT_FLAG.
        /// </summary>
        public int flags;

        /// <summary>
        /// Dữ liệu phụ của packet có thể được cung cấp bởi container.
        /// Packet có thể chứa nhiều loại thông tin phụ.
        /// </summary>
        public nint side_data; // AVPacketSideData*

        /// <summary>
        /// Số lượng phần tử trong side_data.
        /// </summary>
        public int side_data_elems;

        /// <summary>
        /// Thời lượng của packet này tính theo đơn vị AVStream->time_base, 0 nếu không xác định.
        /// Bằng next_pts - this_pts theo thứ tự trình chiếu.
        /// </summary>
        public long duration;

        /// <summary>
        /// Vị trí byte trong luồng, -1 nếu không xác định.
        /// </summary>
        public long pos;

        /// <summary>
        /// Dành cho dữ liệu riêng tư của người dùng.
        /// </summary>
        public nint opaque;

        /// <summary>
        /// AVBufferRef để người dùng API tự do sử dụng.
        /// </summary>
        public nint opaque_ref; // AVBufferRef *

        /// <summary>
        /// Cơ sở thời gian của các dấu thời gian trong packet.
        /// </summary>
        public AVRational time_base;
    }
}
