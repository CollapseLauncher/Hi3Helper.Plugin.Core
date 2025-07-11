using System;
using System.Buffers;
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
        internal delegate int WriteExceptionCallback(Exception exception, Span<byte> messageBuffer);

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

        private static int WriteHttpRequestExceptionInfo(Exception exception, Span<byte> buffer)
        {
            if (exception is not HttpRequestException httpRequestEx) return 0;

            HttpStatusCode statusCode = httpRequestEx.StatusCode ?? HttpStatusCode.NotAcceptable;
            int            hResult    = httpRequestEx.HResult;
            if (hResult == 0)
            {
                hResult = Marshal.GetHRForLastWin32Error();
            }

            int written = WriteAppendInfo(((int)statusCode).ToString(), buffer);
            buffer  =  buffer[written..];
            written += WriteAppendInfo(hResult.ToString(), buffer);

            return written;
        }

        private static int WriteIOExceptionInfo(Exception exception, Span<byte> buffer)
        {
            if (exception is not IOException ioException) return 0;

            int hResult = ioException.HResult;
            if (hResult == 0)
            {
                hResult = Marshal.GetHRForLastWin32Error();
            }

            return WriteAppendInfo(hResult.ToString(), buffer);
        }

        private static int WriteObjectDisposedExceptionInfo(Exception exception, Span<byte> buffer)
            => exception is not ObjectDisposedException objectDisposedEx ? 0 : WriteAppendInfo(objectDisposedEx.ObjectName, buffer);

        private static int WriteSocketExceptionInfo(Exception exception, Span<byte> buffer)
        {
            if (exception is not SocketException socketEx) return 0;

            int nativeErrorCode = socketEx.NativeErrorCode;
            int hResult         = socketEx.HResult;

            if (hResult == 0)
            {
                hResult = Marshal.GetHRForLastWin32Error();
            }

            int written = WriteAppendInfo(nativeErrorCode.ToString(), buffer);
            buffer  =  buffer[written..];
            written += WriteAppendInfo(hResult.ToString(), buffer);

            return written;
        }

        private static int WriteTypeInitializationExceptionInfo(Exception exception, Span<byte> buffer)
            => exception is not TypeInitializationException typeInitEx ? 0 : WriteAppendInfo(typeInitEx.TypeName, buffer);

        private static int WriteAppendInfo(string info, Span<byte> buffer)
        {
            if (buffer.Length == 0)
            {
                return 0;
            }

            buffer[0] = (byte)ComAsyncException.ExExceptionInfoSeparator;
            buffer    = buffer[1..];

            Encoding.UTF8.TryGetBytes(info, buffer, out int bytesWritten);
            return bytesWritten;
        }
    }

    internal static string TryGetExceptionNameFromMessage(string? message)
    {
        const string DefaultName = "Exception";

        if (string.IsNullOrEmpty(message))
        {
            return DefaultName;
        }

        Span<Range> range = stackalloc Range[4];
        int len = message.AsSpan().Split(range, '_', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (len <= 0)
        {
            return DefaultName;
        }

        string tryExceptionName = message[range[0]] + DefaultName;
        return ExceptionNames.ExceptionCreateDelegateLookup.ContainsKey(tryExceptionName) ? tryExceptionName : DefaultName;
    }

    internal static void WriteExceptionInfo(Exception exception, ref ComAsyncException result)
    {
        string exceptionName =
#if MANUALCOM
            TryGetExceptionNameFromMessage(exception.Message);
#else
            exception.GetType().Name;
#endif
        string  exceptionMessage    = exception.Message;
        string? exceptionStackTrace = exception.StackTrace;
        string? exceptionInfo       = null;

        if (ExceptionNames.ExceptionWriteInfoDelegateLookup.TryGetValue(exceptionName,
            out ExceptionNames.WriteExceptionCallback? writeExceptionInfoCallback))
        {
            byte[] writeInfoBuffer = ArrayPool<byte>.Shared.Rent(1 << 10);
            try
            {
                int written = writeExceptionInfoCallback(exception, writeInfoBuffer);
                if (written != 0)
                {
                    exceptionInfo = Encoding.UTF8.GetString(writeInfoBuffer.AsSpan(0, written));
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(writeInfoBuffer);
            }
        }

        result.Write(exceptionName, exceptionInfo, exceptionMessage, exceptionStackTrace);
    }
}
