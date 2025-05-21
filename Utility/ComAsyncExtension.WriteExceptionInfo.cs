using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
// ReSharper disable InconsistentNaming

namespace Hi3Helper.Plugin.Core.Utility;

public static partial class ComAsyncExtension
{
    private static partial class ExceptionNames
    {
        internal delegate void WriteExceptionCallback(Exception exception, Span<byte> messageBuffer);

        private static readonly Dictionary<string, WriteExceptionCallback> ExceptionWriteInfoDelegate
            = new(StringComparer.OrdinalIgnoreCase)
            {
                { ExNameHttpRequestException, WriteHttpRequestExceptionInfo },
                { ExNameIOException, WriteIOExceptionInfo },
                { ExNameObjectDisposedException, WriteObjectDisposedExceptionInfo },
                { ExNameSocketException, WriteSocketExceptionInfo },
                { ExNameTypeInitializationException, WriteTypeInitializationExceptionInfo }
            };

        internal static readonly Dictionary<string, WriteExceptionCallback>.AlternateLookup<ReadOnlySpan<char>>
            ExceptionWriteInfoDelegateLookup = ExceptionWriteInfoDelegate.GetAlternateLookup<ReadOnlySpan<char>>();

        private static void WriteHttpRequestExceptionInfo(Exception exception, Span<byte> buffer)
        {
            if (exception is not HttpRequestException httpRequestEx) return;

            HttpStatusCode statusCode = httpRequestEx.StatusCode ?? HttpStatusCode.NotAcceptable;
            int hResult = httpRequestEx.HResult;
            if (hResult == 0)
            {
                hResult = Marshal.GetHRForLastWin32Error();
            }

            int written = WriteAppendInfo(((int)statusCode).ToString(), buffer);
            buffer = buffer[written..];
            _ = WriteAppendInfo(hResult.ToString(), buffer);
        }

        private static void WriteIOExceptionInfo(Exception exception, Span<byte> buffer)
        {
            if (exception is not IOException ioException) return;

            int hResult = ioException.HResult;
            if (hResult == 0)
            {
                hResult = Marshal.GetHRForLastWin32Error();
            }

            _ = WriteAppendInfo(hResult.ToString(), buffer);
        }

        private static void WriteObjectDisposedExceptionInfo(Exception exception, Span<byte> buffer)
        {
            if (exception is not ObjectDisposedException objectDisposedEx) return;

            _ = WriteAppendInfo(objectDisposedEx.ObjectName, buffer);
        }

        private static void WriteSocketExceptionInfo(Exception exception, Span<byte> buffer)
        {
            if (exception is not SocketException socketEx) return;

            int nativeErrorCode = socketEx.NativeErrorCode;
            int hResult         = socketEx.HResult;

            if (hResult == 0)
            {
                hResult = Marshal.GetHRForLastWin32Error();
            }

            int written = WriteAppendInfo(nativeErrorCode.ToString(), buffer);
            buffer = buffer[written..];
            _ = WriteAppendInfo(hResult.ToString(), buffer);
        }

        private static void WriteTypeInitializationExceptionInfo(Exception exception, Span<byte> buffer)
        {
            if (exception is not TypeInitializationException typeInitEx) return;

            _ = WriteAppendInfo(typeInitEx.TypeName, buffer);
        }

        private static int WriteAppendInfo(string info, Span<byte> buffer)
        {
            if (buffer.Length == 0)
            {
                return 0;
            }

            buffer[0] = (byte)ComAsyncException.ExceptionInfoSeparator;
            buffer = buffer[1..];

            Encoding.UTF8.TryGetBytes(info, buffer, out int bytesWritten);
            return bytesWritten;
        }
    }

    internal static void WriteExceptionInfo(Exception exception, ref ComAsyncException result)
    {
        // Allocate struct and its buffer
        result = new ComAsyncException();

        string exceptionName        = exception.GetType().Name;
        string exceptionMessage     = exception.Message;
        string? exceptionStackTrace = exception.StackTrace;

        Span<byte> exceptionNameSpan       = result.ExceptionTypeByName.AsSpan()[..^1];
        Span<byte> exceptionInfo           = result.ExceptionInfo.AsSpan()[..^1];
        Span<byte> exceptionMessageSpan    = result.ExceptionMessage.AsSpan()[..^1];
        Span<byte> exceptionStackTraceSpan = result.ExceptionStackTrace.AsSpan()[..^1];

        _ = Encoding.UTF8.TryGetBytes(exceptionName, exceptionNameSpan, out _);
        _ = Encoding.UTF8.TryGetBytes(exceptionMessage, exceptionMessageSpan, out _);
        _ = Encoding.UTF8.TryGetBytes(exceptionStackTrace, exceptionStackTraceSpan, out _);

        if (ExceptionNames.ExceptionWriteInfoDelegateLookup.TryGetValue(exceptionName,
            out ExceptionNames.WriteExceptionCallback? writeExceptionInfoCallback))
        {
            writeExceptionInfoCallback(exception, exceptionInfo);
        }}
}
