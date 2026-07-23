using System.Runtime.InteropServices;

namespace Kztek.VideoPlayer.Avalonia.PInvoke
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct AVFormatContext
    {
        public nint av_class;
        public nint iformat; // const struct AVInputFormat*
        public nint oformat; // const struct AVOutputFormat*
        public nint priv_data;
        public nint pb;      // AVIOContext*
        public int ctx_flags;
        public uint nb_streams;

        // Con trỏ tới một mảng các con trỏ (AVStream**)
        public nint streams;

        // ... bỏ qua nhiều trường ở giữa để cho đơn giản ...
        // Để đến đúng được các trường bên dưới, ta cần thêm các vùng đệm (padding)
        // Kích thước chính xác cần có kiến thức sâu về packing của compiler,
        // nhưng ta có thể tạm thời bỏ qua và chỉ truy cập các trường đầu tiên.
        // Trong ví dụ này, chúng ta sẽ dừng ở nb_streams.

        // Các trường sau đây nằm ở vị trí khác, ta sẽ truy cập chúng sau khi có
        // định nghĩa struct đầy đủ hơn.
        // public long start_time;
        // public long duration;
        // public long bit_rate;
    }
}
