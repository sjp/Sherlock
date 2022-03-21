using System;
using EnumsNET;

namespace SJP.Sherlock;

internal static class Win32Extensions
{
    public static NativeMethods.WinErrorCode ToErrorCode(this int errorCode)
    {
        if (!Enums.TryToObject<NativeMethods.WinErrorCode>(errorCode, out var result))
            throw new InvalidCastException($"Unable to convert result code of { errorCode } to a known error code.");

        return result;
    }
}