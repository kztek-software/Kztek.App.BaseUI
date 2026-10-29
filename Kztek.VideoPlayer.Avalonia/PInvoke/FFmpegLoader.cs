using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Kztek.VideoPlayer.Avalonia.PInvoke
{
    /// <summary>
    /// Custom <see cref="NativeLibrary.SetDllImportResolver"/> cho <see cref="FFMPEG"/> — cho phép
    /// <c>[DllImport]</c> dùng tên logic (VD "libavcodec") thay vì đường dẫn/tên file cố định theo OS.
    /// Windows: map tên logic → file .dll cụ thể trong <c>Players/FFMPEG/Resource/</c> (đã đóng gói sẵn).
    /// Linux: tìm <c>lib&lt;name&gt;.so</c> hoặc bản có version suffix (VD <c>libavcodec.so.61</c>) trong
    /// cùng thư mục — CHƯA có file .so nào được đóng gói trong repo (D13 ADR — out-of-scope), resolver
    /// chỉ chuẩn bị hạ tầng để khi nào bổ sung .so thật thì chạy được ngay, không cần sửa lại code này.
    /// </summary>
    public static class FFmpegLoader
    {
        private static bool _initialized;
        private static readonly object InitLock = new();

        static FFmpegLoader()
        {
            NativeLibrary.SetDllImportResolver(typeof(FFMPEG).Assembly, (name, asm, path) =>
            {
                if (OperatingSystem.IsLinux())
                {
                    return TryResolveLinuxLibrary(name);
                }

                return TryResolveWindowsLibrary(name);
            });
        }

        /// <summary>Gọi để kích hoạt static constructor (đăng ký resolver) trước lần PInvoke đầu tiên.</summary>
        public static void Init()
        {
            if (_initialized)
            {
                return;
            }

            lock (InitLock)
            {
                _initialized = true;
            }
        }

        private static nint TryResolveLinuxLibrary(string name)
        {
            foreach (var candidate in GetLinuxCandidates(name))
            {
                if (TryLoad(candidate, out var handle))
                {
                    return handle;
                }
            }

            return nint.Zero;
        }

        private static nint TryResolveWindowsLibrary(string name)
        {
            var map = new[]
            {
                (Logical: "libavformat", File: "avformat-61.dll"),
                (Logical: "libavcodec", File: "avcodec-61.dll"),
                (Logical: "libavutil", File: "avutil-59.dll"),
                (Logical: "libswscale", File: "swscale-8.dll"),
                (Logical: "libswresample", File: "swresample-5.dll")
            };

            foreach (var item in map)
            {
                if (!string.Equals(name, item.Logical, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                foreach (var directory in GetSearchDirectories())
                {
                    var candidate = Path.Combine(directory, item.File);
                    if (TryLoad(candidate, out var handle))
                    {
                        return handle;
                    }
                }
            }

            return nint.Zero;
        }

        private static IEnumerable<string> GetLinuxCandidates(string name)
        {
            var normalized = name.StartsWith("lib", StringComparison.OrdinalIgnoreCase) ? name : $"lib{name}";
            var prefixes = new[]
            {
                normalized,
                $"{normalized}.so"
            };

            foreach (var directory in GetSearchDirectories())
            {
                if (!Directory.Exists(directory))
                {
                    continue;
                }

                foreach (var prefix in prefixes)
                {
                    var directPath = Path.Combine(directory, prefix);
                    yield return directPath;

                    foreach (var versioned in Directory.EnumerateFiles(directory, $"{prefix}*", SearchOption.TopDirectoryOnly))
                    {
                        yield return versioned;
                    }
                }
            }
        }

        private static IEnumerable<string> GetSearchDirectories()
        {
            var baseDir = AppContext.BaseDirectory;
            yield return baseDir;
            // Vị trí thật của file FFmpeg native đã đóng gói (csproj <None Update> CopyToOutputDirectory).
            yield return Path.Combine(baseDir, "runtimes", "win-x64", "native");
            // Bản FFmpeg dùng chung với Kztek.Cameras.Avalonia (app chỉ giữ 1 bản ở đây, không nhân đôi ra gốc/runtimes).
            yield return Path.Combine(baseDir, "Players", "FFMPEG", "Resource");
            yield return Path.Combine(baseDir, "Native");
            yield return Path.Combine(baseDir, "lib");
        }

        private static bool TryLoad(string path, out nint handle)
        {
            handle = nint.Zero;
            if (!File.Exists(path))
            {
                return false;
            }

            try
            {
                handle = NativeLibrary.Load(path);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
