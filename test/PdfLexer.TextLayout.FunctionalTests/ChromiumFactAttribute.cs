using System.Runtime.InteropServices;
using Xunit;

namespace PdfLexer.TextLayout.FunctionalTests;

public sealed class ChromiumFactAttribute : FactAttribute
{
    private static readonly string[] LinuxLibraries =
    {
        "libglib-2.0.so.0",
        "libnss3.so",
        "libnspr4.so",
        "libatk-1.0.so.0",
        "libatk-bridge-2.0.so.0",
        "libcups.so.2",
        "libdrm.so.2",
        "libdbus-1.so.3",
        "libxkbcommon.so.0",
        "libx11.so.6",
        "libxcomposite.so.1",
        "libxdamage.so.1",
        "libxext.so.6",
        "libxfixes.so.3",
        "libxrandr.so.2",
        "libgbm.so.1",
        "libpango-1.0.so.0",
        "libcairo.so.2",
        "libasound.so.2"
    };

    public ChromiumFactAttribute()
    {
        Skip = GetSkipReason();
    }

    private static string? GetSkipReason()
    {
        if (!OperatingSystem.IsLinux())
        {
            return null;
        }

        foreach (var library in LinuxLibraries)
        {
            if (!NativeLibrary.TryLoad(library, out var handle))
            {
                return $"Chromium functional tests require native dependency '{library}'.";
            }

            NativeLibrary.Free(handle);
        }

        return null;
    }
}
