using Hi3Helper.Plugin.Core.Utility;
using System;

namespace Hi3Helper.Plugin.Core;

/// <summary>
/// This struct is used to store the exception information invoked from the plugin's asynchronous methods.
/// </summary>
public unsafe struct ComAsyncException()
    : IDisposable, IInitializableStruct
{
    public const int  ExExceptionTypeNameMaxLength   = 64;
    public const int  ExExceptionInfoMaxLength       = 64;
    public const int  ExExceptionMessageMaxLength    = 384;
    public const int  ExExceptionStackTraceMaxLength = 3568;
    public const char ExExceptionInfoSeparator       = '$';

    public void InitInner()
    {
        _exceptionTypeByName = Mem.Alloc<byte>(ExExceptionTypeNameMaxLength);
        _exceptionInfo       = Mem.Alloc<byte>(ExExceptionInfoMaxLength);
        _exceptionMessage    = Mem.Alloc<byte>(ExExceptionMessageMaxLength);
        _exceptionStackTrace = Mem.Alloc<byte>(ExExceptionStackTraceMaxLength);
    }

    private byte* _exceptionTypeByName = null;
    private byte* _exceptionInfo       = null;
    private byte* _exceptionMessage    = null;
    private byte* _exceptionStackTrace = null;
    private int   _isFreed             = 0;

    /// <summary>
    /// Exception Type Name (without namespace). For example: "InvalidOperationException"
    /// </summary>
    public readonly PluginDisposableMemory<byte> ExceptionTypeByName => new(_exceptionTypeByName, ExExceptionTypeNameMaxLength);

    /// <summary>
    /// Exception Type Info. This stores some information which is required to parse the exception.
    /// </summary>
    public readonly PluginDisposableMemory<byte> ExceptionInfo => new(_exceptionInfo, ExExceptionInfoMaxLength);

    /// <summary>
    /// Exception Message.
    /// </summary>
    public readonly PluginDisposableMemory<byte> ExceptionMessage => new(_exceptionMessage, ExExceptionMessageMaxLength);

    /// <summary>
    /// Exception Stack Trace (Remote Stack Trace from the Plugin).
    /// </summary>
    public readonly PluginDisposableMemory<byte> ExceptionStackTrace => new(_exceptionStackTrace, ExExceptionStackTraceMaxLength);

    public void Dispose()
    {
        if (_isFreed == 1) return;

        Mem.Free(_exceptionTypeByName);
        Mem.Free(_exceptionInfo);
        Mem.Free(_exceptionMessage);
        Mem.Free(_exceptionStackTrace);

        _isFreed = 1;
    }

    /// <summary>
    /// Gets the exception from the span handle.
    /// </summary>
    /// <param name="exceptionMemory">The span handle in which stores the <see cref="ComAsyncException"/> struct.</param>
    /// <returns>Nullable <see cref="Exception"/> instance. Returns <c>null</c> if no exception is being found.</returns>
    public static Exception? GetExceptionFromHandle(PluginDisposableMemory<ComAsyncException> exceptionMemory)
    {
        if (exceptionMemory.IsEmpty)
        {
            return null;
        }

        Exception? parentException = ComAsyncExtension.GetExceptionFromInfo(ref exceptionMemory[0]);
        if (exceptionMemory.Length == 1)
        {
            return parentException;
        }

        if (parentException == null)
        {
            return null;
        }

        Exception? innerException = null;
        Exception? lastInnerException = null;

        for (int i = 1; i < exceptionMemory.Length; i++)
        {
            ref ComAsyncException innerExceptionHandle = ref exceptionMemory[i];
            Exception? innerExceptionCurrent = ComAsyncExtension.GetExceptionFromInfo(ref innerExceptionHandle);

            if (innerExceptionCurrent == null)
            {
                return parentException;
            }

            if (lastInnerException != null)
            {
                lastInnerException.SetExceptionInnerException() = innerExceptionCurrent;
            }

            innerException ??= innerExceptionCurrent;
            lastInnerException = innerExceptionCurrent;
        }

        if (innerException != null)
        {
            parentException.SetExceptionInnerException() = innerException;
        }

        return parentException;
    }
}
