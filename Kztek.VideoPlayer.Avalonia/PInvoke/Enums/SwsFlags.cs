namespace Kztek.VideoPlayer.Avalonia.PInvoke
{
    // Constants (partial)
    /// <summary>
    /// Scaling algorithm flags.
    /// </summary>
    public static class SwsFlags
    {
        public const int SWS_FAST_BILINEAR = 1;    // Fast but low-quality bilinear
        public const int SWS_BILINEAR = 2;         // Standard bilinear
        public const int SWS_BICUBIC = 4;          // Bicubic (better quality)
        public const int SWS_POINT = 0x10;         // Nearest neighbor (pixel duplication)
        public const int SWS_LANCZOS = 0x200;      // Lanczos (high-quality)
        public const int SWS_FULL_CHR_H_INT = 0x2000; // Full chroma interpolation horizontal when upscaling
        public const int SWS_ACCURATE_RND = 0x40000;  // Use accurate rounding
        public const int SWS_BITEXACT = 0x80000;      // Bit-exact output (useful for regression tests)
    }
}
