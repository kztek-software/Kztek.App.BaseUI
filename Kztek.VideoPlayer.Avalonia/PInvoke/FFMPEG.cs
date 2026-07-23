using Kztek.VideoPlayer.Avalonia.PInvoke;

using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Kztek.VideoPlayer.Avalonia.PInvoke
{
    /// <summary>
    /// PInvoke bindings cho FFmpeg native libraries (avcodec/avformat/avutil/swscale).
    /// Tên thư viện dùng trong <c>[DllImport]</c> là TÊN LOGIC (VD "libavcodec"), được
    /// <see cref="FFmpegLoader"/> (custom <c>DllImportResolver</c>) resolve sang file thật
    /// theo OS: Windows → .dll trong <c>Players/FFMPEG/Resource/</c> (đã đóng gói sẵn),
    /// Linux → <c>lib*.so</c> cùng thư mục — KHÔNG đóng gói trong repo (2026-07-16: libavcodec.so
    /// > 100MB, GitHub từ chối push) — copy thủ công vào đó tại máy dev/deploy Linux trước khi
    /// build/publish. LƯU Ý: bản .so Linux đã thử là FFmpeg major version 62, struct P/Invoke
    /// trong <c>Objects/*.cs</c> viết theo layout version 61 (Windows) — ABI CHƯA verify tương
    /// thích trên Linux thật (ADR §7 R9).
    /// </summary>
    public class FFMPEG
    {
        static FFMPEG()
        {
            FFmpegLoader.Init();
        }

        private const string LibSwscale = "libswscale";
        private const string AvUtilDll = "libavutil";
        private const string AvFormatDll = "libavformat";
        private const string AvCodecDll = "libavcodec";

        // Delegate cho callback get_format
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate AVPixelFormat GetFormatFunc(IntPtr ctx, IntPtr fmts);
        #region Swscale

        /// <summary>Return LIBSWSCALE_VERSION_INT</summary>
        [DllImport(LibSwscale, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint swscale_version();

        /// <summary>Get build-time configuration string</summary>
        [DllImport(LibSwscale, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint swscale_configuration();

        /// <summary>Return swscale license string</summary>
        [DllImport(LibSwscale, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint swscale_license();

        /// <summary>Allocate empty SwsContext (must be initialized)</summary>
        [DllImport(LibSwscale, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint sws_alloc_context();

        /// <summary>Initialize a pre-allocated SwsContext</summary>
        [DllImport(LibSwscale, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sws_init_context(nint ctx, nint srcFilter, nint dstFilter);

        /// <summary>Free an allocated SwsContext</summary>
        [DllImport(LibSwscale, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sws_freeContext(nint ctx);

        /// <summary>
        /// Create and initialize a new SwsContext for converting from one format to another.
        /// </summary>
        [DllImport(LibSwscale, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint sws_getContext(int srcW, int srcH, int srcFormat,
                                                   int dstW, int dstH, int dstFormat,
                                                   int flags, nint srcFilter, nint dstFilter, nint param);

        /// <summary>
        /// Perform image scaling from src to dst buffers.
        /// </summary>
        [DllImport(LibSwscale, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sws_scale(nint ctx,
            nint[] srcSlice, int[] srcStride,
            int srcSliceY, int srcSliceH,
            nint[] dst, int[] dstStride);


        [DllImport(LibSwscale, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int sws_scale(
            IntPtr c,
            IntPtr* srcSlice,   // const uint8_t* const srcSlice[]
            int* srcStride,  // const int srcStride[]
            int srcSliceY,
            int srcSliceH,
            IntPtr* dst,        // uint8_t* const dst[]
            int* dstStride); // const int dstStride[]


        /// <summary>Return color space coefficients (e.g. for ITU709)</summary>
        [DllImport(LibSwscale, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint sws_getCoefficients(int colorspace);

        /// <summary>Create empty vector</summary>
        [DllImport(LibSwscale, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint sws_allocVec(int length);

        /// <summary>Create Gaussian filter vector</summary>
        [DllImport(LibSwscale, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint sws_getGaussianVec(double variance, double quality);

        /// <summary>Multiply vector by scalar</summary>
        [DllImport(LibSwscale, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sws_scaleVec(nint vec, double scalar);

        /// <summary>Normalize vector so that its sum equals target height</summary>
        [DllImport(LibSwscale, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sws_normalizeVec(nint vec, double height);

        /// <summary>Free filter vector</summary>
        [DllImport(LibSwscale, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sws_freeVec(nint vec);

        /// <summary>Create default sharpening/blur filters</summary>
        [DllImport(LibSwscale, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint sws_getDefaultFilter(float lumaGBlur, float chromaGBlur,
                                                         float lumaSharpen, float chromaSharpen,
                                                         float chromaHShift, float chromaVShift,
                                                         int verbose);

        /// <summary>Free SwsFilter</summary>
        [DllImport(LibSwscale, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sws_freeFilter(nint filter);

        /// <summary>Set YUV to RGB conversion parameters</summary>
        [DllImport(LibSwscale, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sws_setColorspaceDetails(nint ctx, nint invTable, int srcRange,
                                                          nint table, int dstRange,
                                                          int brightness, int contrast, int saturation);

        /// <summary>Get YUV to RGB conversion parameters</summary>
        [DllImport(LibSwscale, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sws_getColorspaceDetails(nint ctx,
                                                          out nint invTable, out int srcRange,
                                                          out nint table, out int dstRange,
                                                          out int brightness, out int contrast, out int saturation);

        /// <summary>Get or reuse cached context</summary>
        [DllImport(LibSwscale, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint sws_getCachedContext(nint context,
            int srcW, int srcH, int srcFormat,
            int dstW, int dstH, int dstFormat,
            int flags, nint srcFilter, nint dstFilter, nint param);
        #endregion END Swscale

        #region AVUTIL
        /// <summary>
        /// Giải phóng một AVDictionary.
        /// </summary>
        [DllImport(AvUtilDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void av_dict_free(ref nint m);
        #region Constants
        // Từ avutil.h
        public const long AV_NOPTS_VALUE = -9223372036854775808;
        public const int AV_TIME_BASE = 1000000;

        // Từ log.h
        public const int AV_LOG_QUIET = -8;
        public const int AV_LOG_INFO = 32;
        public const int AV_LOG_DEBUG = 48;

        // Từ opt.h
        public const int AV_OPT_SEARCH_CHILDREN = 1 << 0;

        // ... và các hằng số khác
        #endregion
        /// <summary>
        /// Thiết lập một cặp key/value trong AVDictionary.
        /// </summary>
        /// <param name="pm">Con trỏ tới con trỏ AVDictionary. Nếu *pm là NULL, một dictionary mới sẽ được cấp phát.</param>
        /// <param name="key">Khóa của mục.</param>
        /// <param name="value">Giá trị của mục.</param>
        /// <param name="flags">Một tập hợp các cờ AV_DICT_*.</param>
        /// <returns>0 nếu thành công, hoặc mã lỗi âm.</returns>
        [DllImport(AvUtilDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int av_dict_set(ref nint pm, string key, string value, int flags);

        #region PInvoke Private Methods (for string returns)
        [DllImport(AvUtilDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern nint avutil_configuration();

        [DllImport(AvUtilDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern nint avutil_license();

        //[DllImport(AvUtilDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //private static extern IntPtr av_get_pix_fmt_name(AVPixelFormat pix_fmt);
        #endregion

        #region Public Wrapper Methods
        /// <summary>
        /// Trả về chuỗi cấu hình lúc biên dịch của libavutil.
        /// </summary>
        public static string GetConfiguration() => Marshal.PtrToStringAnsi(avutil_configuration());

        /// <summary>
        /// Trả về chuỗi license của libavutil.
        /// </summary>
        public static string GetLicense() => Marshal.PtrToStringAnsi(avutil_license());

        /// <summary>
        /// Lấy tên của một định dạng pixel.
        /// </summary>
        //public static string GetPixFmtName(AVPixelFormat pix_fmt) => Marshal.PtrToStringAnsi(av_get_pix_fmt_name(pix_fmt));

        /// <summary>
        /// Tái triển khai của av_toupper từ avstring.h.
        /// </summary>
        public static int AvToupper(int c)
        {
            if (c >= 'a' && c <= 'z') return c ^ 0x20;
            return c;
        }
        #endregion

        #region Direct PInvoke Functions
        // --- Các hàm phiên bản ---
        [DllImport(AvUtilDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint avutil_version();

        // --- Các hàm log ---
        [DllImport(AvUtilDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void av_log_set_level(int level);
        [DllImport(AvUtilDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int av_log_get_level();

        // --- Các hàm quản lý bộ nhớ ---
        [DllImport(AvUtilDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint av_malloc(ulong size);
        [DllImport(AvUtilDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void av_free(nint ptr);
        [DllImport(AvUtilDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void av_freep(ref nint ptr);

        // --- Các hàm lỗi ---
        [DllImport(AvUtilDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int av_strerror(int errnum, IntPtr errbuf, ulong errbuf_size);

        // --- Các hàm Frame/Packet ---
        [DllImport(AvUtilDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int av_frame_ref(IntPtr dst, IntPtr src);

        [DllImport(AvUtilDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint av_frame_alloc();
        [DllImport(AvUtilDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void av_frame_free(ref nint frame);
        [DllImport(AvUtilDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int av_frame_get_buffer(nint frame, int align);
        [DllImport(AvUtilDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void av_frame_unref(nint frame);
        // --- Các hàm băm ---
        [DllImport(AvUtilDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint av_adler32_update(uint adler, byte[] buf, uint len);
        [DllImport(AvUtilDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint av_murmur3_alloc();
        [DllImport(AvUtilDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void av_murmur3_init(nint c);
        [DllImport(AvUtilDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void av_murmur3_update(nint c, byte[] src, ulong len);
        [DllImport(AvUtilDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void av_murmur3_final(nint c, byte[] dst);

        // --- Các hàm AVOptions ---
        [DllImport(AvUtilDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern nint av_opt_find(nint obj, string name, string unit, int opt_flags, int search_flags);
        [DllImport(AvUtilDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint av_opt_next(nint obj, nint prev);
        [DllImport(AvUtilDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int av_opt_set_int(nint obj, string name, long val, int search_flags);
        [DllImport(AvUtilDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int av_opt_get_int(nint obj, string name, int search_flags, out long out_val);

        // (Và rất nhiều hàm P/Invoke khác mà chúng ta đã định nghĩa)
        #endregion
        #endregion END AVUTIL

        #region AvFORMAT
        #region Version and General Info
        /// <summary>
        /// Trả về hằng số phiên bản của libavformat.
        /// </summary>
        [DllImport(AvFormatDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint avformat_version();

        [DllImport(AvFormatDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern nint avformat_configuration();

        [DllImport(AvFormatDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern nint avformat_license();

        /// <summary>
        /// Khởi tạo các thư viện mạng. Chỉ cần gọi một lần.
        /// </summary>
        [DllImport(AvFormatDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int avformat_network_init();

        /// <summary>
        /// Hủy khởi tạo các thư viện mạng.
        /// </summary>
        [DllImport(AvFormatDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int avformat_network_deinit();
        #endregion

        #region Muxer/Demuxer Iteration
        /// <summary>
        /// Lặp qua tất cả các demuxer đã đăng ký.
        /// </summary>
        /// <param name="opaque">Một con trỏ private đại diện cho trạng thái lặp. Phải là ref IntPtr(0) trong lần gọi đầu tiên.</param>
        /// <returns>Con trỏ tới AVInputFormat tiếp theo, hoặc IntPtr.Zero nếu hết.</returns>
        [DllImport(AvFormatDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint av_demuxer_iterate(ref nint opaque);

        /// <summary>
        /// Lặp qua tất cả các muxer đã đăng ký.
        /// </summary>
        /// <param name="opaque">Một con trỏ private đại diện cho trạng thái lặp. Phải là ref IntPtr(0) trong lần gọi đầu tiên.</param>
        /// <returns>Con trỏ tới AVOutputFormat tiếp theo, hoặc IntPtr.Zero nếu hết.</returns>
        [DllImport(AvFormatDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint av_muxer_iterate(ref nint opaque);
        #endregion

        #region Context Management
        /// <summary>
        /// Cấp phát một AVFormatContext.
        /// </summary>
        [DllImport(AvFormatDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint avformat_alloc_context();

        /// <summary>
        /// Giải phóng một AVFormatContext và tất cả các stream của nó.
        /// </summary>
        /// <param name="s">Con trỏ tới AVFormatContext cần giải phóng.</param>
        [DllImport(AvFormatDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void avformat_free_context(nint s);

        /// <summary>
        /// Mở một stream đầu vào và đọc header.
        /// </summary>
        [DllImport(AvFormatDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int avformat_open_input(ref nint ps, [MarshalAs(UnmanagedType.LPUTF8Str)] string url, nint fmt, ref nint options);

        /// <summary>
        /// Đóng một AVFormatContext đầu vào.
        /// </summary>
        [DllImport(AvFormatDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void avformat_close_input(ref nint s);
        #endregion

        #region Stream and Info
        /// <summary>
        /// Đọc các packet từ file media để lấy thông tin stream.
        /// </summary>
        [DllImport(AvFormatDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int avformat_find_stream_info(nint ic, nint options);

        /// <summary>
        /// In thông tin chi tiết về định dạng input/output ra log.
        /// </summary>
        [DllImport(AvFormatDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void av_dump_format(nint ic, int index, string url, int is_output);

        /// <summary>
        /// Tìm AVInputFormat dựa trên tên ngắn.
        /// </summary>
        [DllImport(AvFormatDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern nint av_find_input_format(string short_name);

        /// <summary>
        /// Tìm stream "tốt nhất" trong file.
        /// </summary>
        [DllImport(AvFormatDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int av_find_best_stream(nint ic, AVMediaType type, int wanted_stream_nb, int related_stream, out nint decoder_ret, int flags);

        /// <summary>
        /// Thêm một stream mới vào media file (khi muxing).
        /// </summary>
        [DllImport(AvFormatDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint avformat_new_stream(nint s, nint c);
        #endregion

        #region Packet Reading and Writing
        /// <summary>
        /// Đọc frame tiếp theo của một stream.
        /// </summary>
        [DllImport(AvFormatDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int av_read_frame(nint s, nint pkt);

        /// <summary>
        /// Ghi header của file media đầu ra.
        /// </summary>
        [DllImport(AvFormatDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int avformat_write_header(nint s, nint options);

        /// <summary>
        /// Ghi một packet vào file media đầu ra (có sắp xếp).
        /// </summary>
        [DllImport(AvFormatDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int av_interleaved_write_frame(nint s, nint pkt);

        /// <summary>
        /// Ghi phần trailer của file media đầu ra.
        /// </summary>
        [DllImport(AvFormatDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int av_write_trailer(nint s);

        /// <summary>
        /// Seek đến một keyframe tại một thời điểm (timestamp) cụ thể.
        /// </summary>
        [DllImport(AvFormatDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int av_seek_frame(nint s, int stream_index, long timestamp, int flags);
        #endregion

        [DllImport(AvFormatDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int av_find_default_stream_index(nint s);
        #endregion END AvFORMAT

        #region AvCodec
        #region Version and General Info
        /// <summary>
        /// Trả về hằng số phiên bản của libavcodec.
        /// </summary>
        [DllImport(AvCodecDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint avcodec_version();

        /// <summary>
        /// Trả về chuỗi cấu hình lúc biên dịch của libavcodec.
        /// </summary>
        [DllImport(AvCodecDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern nint avcodec_configuration();
        /// <summary>
        /// Trả về chuỗi license của libavcodec.
        /// </summary>
        [DllImport(AvCodecDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern nint avcodec_license();
        #endregion

        #region Codec and Parser
        /// <summary>
        /// Tìm một bộ giải mã đã đăng ký theo ID.
        /// </summary>
        /// <param name="id">ID của codec cần tìm.</param>
        /// <returns>Một con trỏ tới AVCodec, hoặc IntPtr.Zero nếu không tìm thấy.</returns>
        [DllImport(AvCodecDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint avcodec_find_decoder(AVCodecID id);
        [DllImport(AvCodecDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void av_packet_unref(nint pkt);
        /// <summary>
        /// Lặp qua tất cả các codec parser đã đăng ký.
        /// </summary>
        /// <param name="opaque">Một con trỏ private đại diện cho trạng thái lặp. Phải là ref IntPtr(0) trong lần gọi đầu tiên.</param>
        /// <returns>Con trỏ tới AVCodecParser tiếp theo, hoặc IntPtr.Zero nếu hết.</returns>
        [DllImport(AvCodecDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint av_parser_iterate(ref nint opaque);

        /// <summary>
        /// Khởi tạo một AVCodecParserContext.
        /// </summary>
        /// <param name="codec_id">ID của codec mà parser này sẽ dùng.</param>
        /// <returns>AVCodecParserContext đã được cấp phát hoặc NULL nếu thất bại.</returns>
        [DllImport(AvCodecDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint av_parser_init(int codec_id);

        /// <summary>
        /// Đóng một AVCodecParserContext.
        /// </summary>
        [DllImport(AvCodecDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void av_parser_close(nint s);
        #endregion

        #region Context Management
        /// <summary>
        /// Cấp phát một AVCodecContext và đặt các trường của nó về giá trị mặc định.
        /// </summary>
        /// <param name="codec">Con trỏ tới AVCodec. Nếu khác null, context sẽ được khởi tạo mặc định cho codec này.</param>
        /// <returns>Một con trỏ tới AVCodecContext, hoặc IntPtr.Zero nếu có lỗi.</returns>
        [DllImport(AvCodecDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint avcodec_alloc_context3(nint codec);

        /// <summary>
        /// Giải phóng một AVCodecContext và tất cả những gì liên quan đến nó.
        /// </summary>
        /// <param name="avctx">Con trỏ trỏ tới con trỏ AVCodecContext cần giải phóng. Sẽ được đặt về NULL sau khi gọi.</param>
        [DllImport(AvCodecDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void avcodec_free_context(ref nint avctx);

        /// <summary>
        /// Sao chép các tham số từ một AVCodecParameters vào một AVCodecContext.
        /// </summary>
        /// <param name="codec_context">Context đích.</param>
        /// <param name="codec_parameters">Con trỏ tới tham số nguồn.</param>
        /// <returns>>=0 nếu thành công, mã lỗi âm nếu thất bại.</returns>
        [DllImport(AvCodecDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int avcodec_parameters_to_context(nint codec_context, nint codec_parameters);

        /// <summary>
        /// Khởi tạo AVCodecContext để sử dụng một AVCodec cụ thể.
        /// </summary>
        /// <param name="avctx">Con trỏ tới AVCodecContext cần khởi tạo.</param>
        /// <param name="codec">Con trỏ tới AVCodec để mở.</param>
        /// <param name="options">Một con trỏ tới AVDictionary chứa các tùy chọn. Có thể là IntPtr.Zero.</param>
        /// <returns>0 nếu thành công, một số âm nếu có lỗi.</returns>
        [DllImport(AvCodecDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int avcodec_open2(nint avctx, nint codec, ref nint options);

        /// <summary>
        /// Trả về giá trị dương nếu context đã được mở.
        /// </summary>
        [DllImport(AvCodecDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int avcodec_is_open(nint s);

        /// <summary>
        /// Xả (flush) các buffer nội bộ của codec.
        /// </summary>
        [DllImport(AvCodecDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void avcodec_flush_buffers(nint avctx);
        #endregion

        #region Packet and Frame Handling
        /// <summary>
        /// Cấp phát một AVPacket với các giá trị mặc định.
        /// </summary>
        [DllImport(AvCodecDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint av_packet_alloc();

        /// <summary>
        /// Giải phóng AVPacket.
        /// </summary>
        [DllImport(AvCodecDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void av_packet_free(ref nint pkt);

        /// <summary>
        /// Gửi một packet thô vào bộ giải mã.
        /// </summary>
        [DllImport(AvCodecDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int avcodec_send_packet(nint avctx, nint avpkt);

        /// <summary>
        /// Nhận một frame đã giải mã từ bộ giải mã.
        /// </summary>
        [DllImport(AvCodecDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int avcodec_receive_frame(nint avctx, nint frame);

        /// <summary>
        /// Gửi một frame thô vào bộ mã hóa.
        /// </summary>
        [DllImport(AvCodecDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int avcodec_send_frame(nint avctx, nint frame);

        /// <summary>
        /// Nhận một packet đã mã hóa từ bộ mã hóa.
        /// </summary>
        [DllImport(AvCodecDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int avcodec_receive_packet(nint avctx, nint avpkt);
        #endregion

        #region Stream Parsers
        /// <summary>
        /// Trích xuất ID luồng bit và kích thước khung từ dữ liệu AC-3.
        /// </summary>
        [DllImport(AvCodecDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int av_ac3_parse_header(byte[] buf, ulong size, out byte bitstream_id, out ushort frame_size);

        /// <summary>
        /// Phân tích một header ADTS và trích xuất số lượng mẫu và số khung AAC.
        /// </summary>
        [DllImport(AvCodecDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int av_adts_header_parse(byte[] buf, out uint samples, out byte frames);
        #endregion

        #region DCT Functions
        /// <summary>
        /// Cấp phát một context AVDCT.
        /// </summary>
        [DllImport(AvCodecDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint avcodec_dct_alloc();

        /// <summary>
        /// Khởi tạo một context AVDCT.
        /// </summary>
        [DllImport(AvCodecDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int avcodec_dct_init(nint ctx);
        #endregion
        #endregion

        public static bool SetParam(int timeOutInMicroSecond, string networkMode, ref nint options)
        {
            av_dict_set(ref options, "timeout", timeOutInMicroSecond.ToString(), 0); // 5 giây timeout
            av_dict_set(ref options, "max_delay", "500000", 0); // 0.5s
            av_dict_set(ref options, "rtsp_transport", networkMode, 0);
            return true;
        }

        public static bool OpenRTSP(string url, out nint context, ref nint options)
        {
            context = avformat_alloc_context();
            if (context == IntPtr.Zero)
                return false;

            if (avformat_open_input(ref context, url, IntPtr.Zero, ref options) != 0)
            {
                // ❗ FREE cả context và options nếu mở thất bại
                if (options != IntPtr.Zero)
                {
                    av_dict_free(ref options);
                    options = IntPtr.Zero;
                }

                if (context != IntPtr.Zero)
                {
                    avformat_free_context(context);
                    context = IntPtr.Zero;
                }

                return false;
            }

            if (options != IntPtr.Zero) av_dict_free(ref options);
            return true;
        }

        //GPU
        [DllImport(AvUtilDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int av_hwdevice_ctx_create(ref IntPtr device_ctx, AVHWDeviceType type, [MarshalAs(UnmanagedType.LPStr)] string device, IntPtr opts, int flags);

        [DllImport(AvUtilDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr av_hwframe_ctx_alloc(IntPtr device_ctx);

        [DllImport(AvUtilDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int av_hwframe_ctx_init(IntPtr ctx);

        [DllImport(AvUtilDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int av_hwframe_transfer_data(IntPtr dst, IntPtr src, int flags);

        [DllImport(AvUtilDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr av_buffer_ref(IntPtr buf);

        [DllImport(AvUtilDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void av_buffer_unref(ref IntPtr buf);

        [DllImport(AvCodecDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr avcodec_find_decoder_by_name([MarshalAs(UnmanagedType.LPStr)] string name);

        public static void SaveVideoWithFFmpeg(string pathSave, MemoryStream[] pre, MemoryStream[] post, float fps)
        {
            // Thêm -loglevel warning để ít spam log hơn
            //string args = $"-y -f image2pipe -c:v mjpeg -r {fps} -i - -c:v libx264 -pix_fmt yuv420p -preset ultrafast -loglevel warning \"{pathSave}\"";

            string args = $"-y -f image2pipe -c:v mjpeg -r {fps.ToString(CultureInfo.InvariantCulture)} -i - " +
                          $"-c:v libx264 -pix_fmt yuv420p -preset ultrafast -loglevel warning \"{pathSave}\"";
            var processInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardError = true, // BẮT BUỘC PHẢI CÓ
                RedirectStandardOutput = false
            };

            using var process = new Process { StartInfo = processInfo };
            // --- BƯỚC 1: Xử lý Log để tránh tắc đường (Quan trọng nhất) ---
            process.ErrorDataReceived += (sender, e) =>
            {
                // Có thể bỏ qua hoặc log ra Debug nếu cần kiểm tra
                // if (e.Data != null) Debug.WriteLine("FFmpeg: " + e.Data);
            };

            process.Start();

            // Bắt đầu hứng log lỗi ngay lập tức, chạy song song với việc ghi
            process.BeginErrorReadLine();

            // --- BƯỚC 2: Ghi dữ liệu ---
            // Sử dụng khối using để đảm bảo stream được Close() tự động ngay khi xong
            using (var stdin = process.StandardInput.BaseStream)
            {
                try
                {
                    // Ghi Pre
                    foreach (var item in pre)
                    {
                        item.WriteTo(stdin);
                    }

                    // Ghi Post
                    foreach (var item in post)
                    {
                        item.WriteTo(stdin);
                        item.Dispose();
                    }

                    // Mẹo nhỏ: Flush để đẩy nốt những byte cuối cùng đi
                    stdin.Flush();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Lỗi ghi Pipe: {ex.Message}");
                }
            }
            // Khi code chạy đến dòng này, khối 'using' kết thúc -> stdin.Close() được gọi.
            // -> FFmpeg nhận được tín hiệu EOF (End Of File) -> Nó bắt đầu kết thúc video.

            // --- BƯỚC 3: Đợi FFmpeg dọn dẹp và tắt hẳn ---
            // Đợi tối đa 5 giây. Nếu quá 5s mà nó chưa xong thì cưỡng chế tắt.
            if (!process.WaitForExit(5000))
            {
                Debug.WriteLine("FFmpeg bị kẹt -> Kill process");
                process.Kill();
            }
        }
    }
}
