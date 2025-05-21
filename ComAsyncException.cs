using Hi3Helper.Plugin.Core.Utility;
using System;

namespace Hi3Helper.Plugin.Core;

public readonly unsafe struct ComAsyncException() : IDisposable
{
    public const int  ExceptionTypeNameMaxLength   = 64;
    public const int  ExceptionInfoMaxLength       = 64;
    public const int  ExceptionMessageMaxLength    = 384;
    public const int  ExceptionStackTraceMaxLength = 3568;
    public const char ExceptionInfoSeparator       = '$';

    private readonly byte* _exceptionTypeByName = Mem.Alloc<byte>(ExceptionTypeNameMaxLength);
    private readonly byte* _exceptionInfo       = Mem.Alloc<byte>(ExceptionInfoMaxLength);
    private readonly byte* _exceptionMessage    = Mem.Alloc<byte>(ExceptionMessageMaxLength);
    private readonly byte* _exceptionStackTrace = Mem.Alloc<byte>(ExceptionStackTraceMaxLength);

    public PluginDisposableMemory<byte> ExceptionTypeByName => new(_exceptionTypeByName, ExceptionTypeNameMaxLength);
    public PluginDisposableMemory<byte> ExceptionInfo       => new(_exceptionInfo, ExceptionInfoMaxLength);
    public PluginDisposableMemory<byte> ExceptionMessage    => new(_exceptionMessage, ExceptionMessageMaxLength);
    public PluginDisposableMemory<byte> ExceptionStackTrace => new(_exceptionStackTrace, ExceptionStackTraceMaxLength);

    public void Dispose()
    {
        Mem.Free(_exceptionTypeByName);
        Mem.Free(_exceptionInfo);
        Mem.Free(_exceptionMessage);
        Mem.Free(_exceptionStackTrace);
    }

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
