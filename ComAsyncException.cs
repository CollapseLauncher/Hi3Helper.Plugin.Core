using Hi3Helper.Plugin.Core.Utility;
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Hi3Helper.Plugin.Core;

/// <summary>
/// This struct is used to store the exception information invoked from the plugin's asynchronous methods.
/// </summary>
[StructLayout(LayoutKind.Explicit)] // Fits to 48 bytes
public unsafe struct ComAsyncException()
    : IDisposable
{
    public const char ExExceptionInfoSeparator = '$';
    
    [FieldOffset(0)]
    private byte _isFreed = 0;

    /// <summary>
    /// Exception Type Name (without namespace). For example: "<see cref="InvalidOperationException"/>"
    /// </summary>
    [FieldOffset(16)]
    public byte* ExceptionTypeByName = null;

    /// <summary>
    /// Exception Type Info. This stores some information which is required to parse the exception.
    /// </summary>
    [FieldOffset(24)]
    public byte* ExceptionInfo = null;

    /// <summary>
    /// Exception Message.
    /// </summary>
    [FieldOffset(32)]
    public byte* ExceptionMessage = null;

    /// <summary>
    /// Exception Stack Trace (Remote Stack Trace from the Plugin).
    /// </summary>
    [FieldOffset(40)]
    public byte* ExceptionStackTrace = null;

    /// <summary>
    /// Create an unmanaged instance of <see cref="ComAsyncException"/> struct from specified strings.
    /// </summary>
    /// <param name="typeByName">
    /// Exception Type Name (without namespace). For example: "<see cref="InvalidOperationException"/>"
    /// </param>
    /// <param name="info">
    /// Exception Type Info. This stores some information which is required to parse the exception.
    /// </param>
    /// <param name="message">
    /// Exception Message.
    /// </param>
    /// <param name="stackTrace">
    /// Exception Stack Trace (Remote Stack Trace from the Plugin).
    /// </param>
    /// <returns>
    /// An instance of <see cref="ComAsyncException"/>
    /// </returns>
    public static ComAsyncException Create(string? typeByName, string? info, string? message, string? stackTrace)
        => new()
        {
            ExceptionTypeByName = Utf8StringMarshaller.ConvertToUnmanaged(typeByName),
            ExceptionInfo       = Utf8StringMarshaller.ConvertToUnmanaged(info),
            ExceptionMessage    = Utf8StringMarshaller.ConvertToUnmanaged(message),
            ExceptionStackTrace = Utf8StringMarshaller.ConvertToUnmanaged(stackTrace)
        };

    /// <summary>
    /// Write the exception data to the current struct instance.
    /// </summary>
    /// <param name="typeByName">
    /// Exception Type Name (without namespace). For example: "<see cref="InvalidOperationException"/>"
    /// </param>
    /// <param name="info">
    /// Exception Type Info. This stores some information which is required to parse the exception.
    /// </param>
    /// <param name="message">
    /// Exception Message.
    /// </param>
    /// <param name="stackTrace">
    /// Exception Stack Trace (Remote Stack Trace from the Plugin).
    /// </param>
    public void Write(string? typeByName, string? info, string? message, string? stackTrace)
    {
        Utf8StringMarshaller.Free(ExceptionTypeByName);
        Utf8StringMarshaller.Free(ExceptionInfo);
        Utf8StringMarshaller.Free(ExceptionMessage);
        Utf8StringMarshaller.Free(ExceptionStackTrace);

        ExceptionTypeByName = Utf8StringMarshaller.ConvertToUnmanaged(typeByName);
        ExceptionInfo       = Utf8StringMarshaller.ConvertToUnmanaged(info);
        ExceptionMessage    = Utf8StringMarshaller.ConvertToUnmanaged(message);
        ExceptionStackTrace = Utf8StringMarshaller.ConvertToUnmanaged(stackTrace);
    }

    public void Dispose()
    {
        if (_isFreed == 1) return;

        Utf8StringMarshaller.Free(ExceptionTypeByName);
        Utf8StringMarshaller.Free(ExceptionInfo);
        Utf8StringMarshaller.Free(ExceptionMessage);
        Utf8StringMarshaller.Free(ExceptionStackTrace);

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
