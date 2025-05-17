using System.Runtime.InteropServices;

namespace Hi3Helper.Plugin.Core;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct ComAsyncResult
{
    public const int ExceptionTypeNameMaxLength = 48;
    public const int ExceptionMessageMaxLength  = 512;

    public nint Handle;

    // TODO: Marshal exception directly instead of determining it by the name (since it's SUPER SLOW!!!).
    public fixed byte ExceptionTypeByName[ExceptionTypeNameMaxLength];
    public fixed byte ExceptionMessage[ExceptionMessageMaxLength];

    public bool IsCancelled;
    public bool IsSuccessful;
    public bool IsFaulty;
}
