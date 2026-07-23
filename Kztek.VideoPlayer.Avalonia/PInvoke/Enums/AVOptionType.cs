namespace Kztek.VideoPlayer.Avalonia.PInvoke
{
    public enum AVOptionType
    {
        /**
         * Underlying C type is unsigned int.
         */
        AV_OPT_TYPE_FLAGS = 1,
        /**
         * Underlying C type is int.
         */
        AV_OPT_TYPE_INT,
        /**
         * Underlying C type is int64_t.
         */
        AV_OPT_TYPE_INT64,
        /**
         * Underlying C type is double.
         */
        AV_OPT_TYPE_DOUBLE,
        /**
         * Underlying C type is float.
         */
        AV_OPT_TYPE_FLOAT,
        /**
         * Underlying C type is a uint8_t* that is either NULL or points to a C
         * string allocated with the av_malloc() family of functions.
         */
        AV_OPT_TYPE_STRING,
        /**
         * Underlying C type is AVRational.
         */
        AV_OPT_TYPE_RATIONAL,
        /**
         * Underlying C type is a uint8_t* that is either NULL or points to an array
         * allocated with the av_malloc() family of functions. The pointer is
         * immediately followed by an int containing the array length in bytes.
         */
        AV_OPT_TYPE_BINARY,
        /**
         * Underlying C type is AVDictionary*.
         */
        AV_OPT_TYPE_DICT,
        /**
         * Underlying C type is uint64_t.
         */
        AV_OPT_TYPE_UINT64,
        /**
         * Special option type for declaring named constants. Does not correspond to
         * an actual field in the object, offset must be 0.
         */
        AV_OPT_TYPE_CONST,
        /**
         * Underlying C type is two consecutive integers.
         */
        AV_OPT_TYPE_IMAGE_SIZE,
        /**
         * Underlying C type is enum AVPixelFormat.
         */
        AV_OPT_TYPE_PIXEL_FMT,
        /**
         * Underlying C type is enum AVSampleFormat.
         */
        AV_OPT_TYPE_SAMPLE_FMT,
        /**
         * Underlying C type is AVRational.
         */
        AV_OPT_TYPE_VIDEO_RATE,
        /**
         * Underlying C type is int64_t.
         */
        AV_OPT_TYPE_DURATION,
        /**
         * Underlying C type is uint8_t[4].
         */
        AV_OPT_TYPE_COLOR,
        /**
         * Underlying C type is int.
         */
        AV_OPT_TYPE_BOOL,
        /**
         * Underlying C type is AVChannelLayout.
         */
        AV_OPT_TYPE_CHLAYOUT,
        /**
         * Underlying C type is unsigned int.
         */
        AV_OPT_TYPE_UINT,

        /**
         * May be combined with another regular option type to declare an array
         * option.
         *
         * For array options, @ref AVOption.offset should refer to a pointer
         * corresponding to the option type. The pointer should be immediately
         * followed by an unsigned int that will store the number of elements in the
         * array.
         */
        AV_OPT_TYPE_FLAG_ARRAY = 1 << 16,
    };

}
