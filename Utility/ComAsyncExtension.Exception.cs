using System;
using System.Buffers;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

// ReSharper disable InconsistentNaming
namespace Hi3Helper.Plugin.Core.Utility;

public static partial class ComAsyncExtension
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_message")]
    private static extern ref string SetExceptionMessage(this Exception exception);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_stackTraceString")]
    private static extern ref string? SetExceptionStackTrace(this Exception exception);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_remoteStackTraceString")]
    private static extern ref string? SetExceptionRemoteStackTrace(this Exception exception);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_innerException")]
    internal static extern ref Exception? SetExceptionInnerException(this Exception exception);

    private static partial class ExceptionNames
    {
        private static readonly Dictionary<string, CreateExceptionCallback<Exception>>
            ExceptionCreateDelegate = new(StringComparer.OrdinalIgnoreCase)
        {
            { ExNameAccessViolationException, CreateAnyException<AccessViolationException> },
            { ExNameAggregateException, CreateAnyException<AggregateException> },
            { ExNameAppDomainUnloadedException, CreateAnyException<AppDomainUnloadedException> },
            { ExNameApplicationException, CreateAnyException<ApplicationException> },
            { ExNameArgumentException, CreateAnyException<ArgumentException> },
            { ExNameArgumentNullException, CreateAnyException<ArgumentNullException> },
            { ExNameArgumentOutOfRangeException, CreateAnyException<ArgumentOutOfRangeException> },
            { ExNameArithmeticException, CreateAnyException<ArithmeticException> },
            { ExNameArrayTypeMismatchException, CreateAnyException<ArrayTypeMismatchException> },
            { ExNameBadImageFormatException, CreateAnyException<BadImageFormatException> },
            { ExNameCannotUnloadAppDomainException, CreateAnyException<CannotUnloadAppDomainException> },
            { ExNameContextMarshalException, CreateAnyException<ContextMarshalException> },
            { ExNameDataMisalignedException, CreateAnyException<DataMisalignedException> },
            { ExNameDivideByZeroException, CreateAnyException<DivideByZeroException> },
            { ExNameDllNotFoundException, CreateAnyException<DllNotFoundException> },
            { ExNameDuplicateWaitObjectException, CreateAnyException<DuplicateWaitObjectException> },
            { ExNameEntryPointNotFoundException, CreateAnyException<EntryPointNotFoundException> },
            { ExNameException, CreateAnyException<Exception> },
            { ExNameFieldAccessException, CreateAnyException<FieldAccessException> },
            { ExNameFileLoadException, CreateAnyException<FileLoadException> },
            { ExNameFileNotFoundException, CreateAnyException<FileNotFoundException> },
            { ExNameFormatException, CreateAnyException<FormatException> },
            { ExNameHttpRequestException, CreateHttpRequestException },
            { ExNameIndexOutOfRangeException, CreateAnyException<IndexOutOfRangeException> },
            { ExNameInsufficientExecutionStackException, CreateAnyException<InsufficientExecutionStackException> },
            { ExNameInsufficientMemoryException, CreateAnyException<InsufficientMemoryException> },
            { ExNameInvalidCastException, CreateAnyException<InvalidCastException> },
            { ExNameInvalidOperationException, CreateAnyException<InvalidOperationException> },
            { ExNameInvalidProgramException, CreateAnyException<InvalidProgramException> },
            { ExNameIOException, CreateIOException },
            { ExNameMemberAccessException, CreateAnyException<MemberAccessException> },
            { ExNameMethodAccessException, CreateAnyException<MethodAccessException> },
            { ExNameMissingFieldException, CreateAnyException<MissingFieldException> },
            { ExNameMissingMemberException, CreateAnyException<MissingMemberException> },
            { ExNameMissingMethodException, CreateAnyException<MissingMethodException> },
            { ExNameMulticastNotSupportedException, CreateAnyException<MulticastNotSupportedException> },
            { ExNameNotFiniteNumberException, CreateAnyException<NotFiniteNumberException> },
            { ExNameNotImplementedException, CreateAnyException<NotImplementedException> },
            { ExNameNotSupportedException, CreateAnyException<NotSupportedException> },
            { ExNameNullReferenceException, CreateAnyException<NullReferenceException> },
            { ExNameObjectDisposedException, CreateObjectDisposedException },
            { ExNameOperationCanceledException, CreateAnyException<OperationCanceledException> },
            { ExNameOutOfMemoryException, CreateAnyException<OutOfMemoryException> },
            { ExNameOverflowException, CreateAnyException<OverflowException> },
            { ExNamePlatformNotSupportedException, CreateAnyException<PlatformNotSupportedException> },
            { ExNameRankException, CreateAnyException<RankException> },
            { ExNameSocketException, CreateSocketException },
            { ExNameStackOverflowException, CreateAnyException<StackOverflowException> },
            { ExNameSystemException, CreateAnyException<SystemException> },
            { ExNameTimeoutException, CreateAnyException<TimeoutException> },
            { ExNameTypeAccessException, CreateAnyException<TypeAccessException> },
            { ExNameTypeInitializationException, CreateTypeInitializationException },
            { ExNameTypeLoadException, CreateAnyException<TypeLoadException> },
            { ExNameTypeUnloadedException, CreateAnyException<TypeUnloadedException> },
            { ExNameUnauthorizedAccessException, CreateAnyException<UnauthorizedAccessException> },
            { ExNameVerificationException, CreateAnyException<VerificationException> },
            { ExNameVersionNotFoundException, CreateAnyException<VersionNotFoundException> }
        };

        internal static readonly Dictionary<string, CreateExceptionCallback<Exception>>.AlternateLookup<ReadOnlySpan<char>>
            ExceptionCreateDelegateLookup = ExceptionCreateDelegate.GetAlternateLookup<ReadOnlySpan<char>>();

        internal delegate T CreateExceptionCallback<out T>(ReadOnlySpan<char> typeExceptionInfo, ReadOnlySpan<char> message) where T : Exception, new();

        private static HttpRequestException CreateHttpRequestException(ReadOnlySpan<char> typeExceptionInfo, ReadOnlySpan<char> message)
        {
            ReadOnlySpan<char> httpStatusCode = GetExceptionInfo(typeExceptionInfo, out int read);
            typeExceptionInfo = typeExceptionInfo[read..];
            ReadOnlySpan<char> hResult = GetExceptionInfo(typeExceptionInfo, out read);

            string messageStr = message.ToString();

            if (int.TryParse(httpStatusCode, out int httpStatusCodeInt) &&
                int.TryParse(hResult, out int hResultInt))
            {
                return new HttpRequestException(messageStr, null, (HttpStatusCode)httpStatusCodeInt)
                {
                    HResult = hResultInt
                };
            }

            return new HttpRequestException(messageStr);
        }

        private static IOException CreateIOException(ReadOnlySpan<char> typeExceptionInfo, ReadOnlySpan<char> message)
        {
            ReadOnlySpan<char> hResult = GetExceptionInfo(typeExceptionInfo, out _);
            if (hResult.Length == 0 || !int.TryParse(hResult, null, out int hResultInt))
            {
                hResultInt = unchecked((int)0x8000FFFF);
            }

            string exceptionMessage = message.ToString();
            IOException exception = new IOException(exceptionMessage, hResultInt);
            return exception;
        }

        private static ObjectDisposedException CreateObjectDisposedException(ReadOnlySpan<char> typeExceptionInfo, ReadOnlySpan<char> message)
        {
            string exceptionMessage = message.ToString();
            string infoObjectName   = GetExceptionInfo(typeExceptionInfo, out _).ToString();

            ObjectDisposedException exception = new ObjectDisposedException(infoObjectName, exceptionMessage);
            return exception;
        }

        private static SocketException CreateSocketException(ReadOnlySpan<char> typeExceptionInfo, ReadOnlySpan<char> message)
        {
            ReadOnlySpan<char> nativeErrorCodeSpan = GetExceptionInfo(typeExceptionInfo, out int read);
            typeExceptionInfo = typeExceptionInfo[read..];
            ReadOnlySpan<char> hResultSpan = GetExceptionInfo(typeExceptionInfo, out _);

            string exceptionMessage = message.ToString();

            if (!int.TryParse(nativeErrorCodeSpan, out int nativeErrorCodeInt) ||
                !int.TryParse(hResultSpan, out int hResultInt))
            {
                return new SocketException(Marshal.GetLastPInvokeError(), exceptionMessage);
            }

            SocketException exception = new SocketException(nativeErrorCodeInt, exceptionMessage)
            {
                HResult = hResultInt
            };
            return exception;

        }

        private static TypeInitializationException CreateTypeInitializationException(ReadOnlySpan<char> typeExceptionInfo, ReadOnlySpan<char> _)
        {
            string                      infoTypeName = GetExceptionInfo(typeExceptionInfo, out int _).ToString();
            TypeInitializationException exception    = new TypeInitializationException(infoTypeName, null);

            return exception;
        }

        private static Exception CreateAnyException<T>(ReadOnlySpan<char> _, ReadOnlySpan<char> message)
            where T : Exception, new()
        {
            Exception exception = new T();
            exception.SetExceptionMessage() = message.ToString();

            return exception;
        }

        private static ReadOnlySpan<char> GetExceptionInfo(ReadOnlySpan<char> typeExceptionInfoBuffer, out int readOffset)
        {
            if (typeExceptionInfoBuffer[0] != ComAsyncException.ExceptionInfoSeparator)
            {
                readOffset = 0;
                return ReadOnlySpan<char>.Empty;
            }

            Span<Range> ranges = stackalloc Range[2];
            int splitRangeLen = typeExceptionInfoBuffer.Split(ranges, ComAsyncException.ExceptionInfoSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (splitRangeLen == 0)
            {
                readOffset = 0;
                return ReadOnlySpan<char>.Empty;
            }

            readOffset = ranges[0].End.Value;
            return typeExceptionInfoBuffer[ranges[0]];
        }
    }

    internal static unsafe Exception? GetExceptionFromInfo(ComAsyncException* result)
    {
        byte* exTypeName   = result->ExceptionTypeByName;
        byte* exInfo       = result->ExceptionInfo;
        byte* exMessage    = result->ExceptionMessage;
        byte* exStackTrace = result->ExceptionStackTrace;

        ReadOnlySpan<byte> exTypeNameSpan   = Mem.CreateSpanFromNullTerminated<byte>(exTypeName);
        ReadOnlySpan<byte> exInfoSpan       = Mem.CreateSpanFromNullTerminated<byte>(exInfo);
        ReadOnlySpan<byte> exMessageSpan    = Mem.CreateSpanFromNullTerminated<byte>(exMessage);
        ReadOnlySpan<byte> exStackTraceSpan = Mem.CreateSpanFromNullTerminated<byte>(exStackTrace);

        if (exTypeNameSpan.IsEmpty)
        {
            return null;
        }

        char[] exInfoBuffer = ArrayPool<char>.Shared.Rent(ComAsyncException.ExceptionTypeNameMaxLength +
                                                          ComAsyncException.ExceptionInfoMaxLength +
                                                          ComAsyncException.ExceptionMessageMaxLength +
                                                          ComAsyncException.ExceptionStackTraceMaxLength);
        try
        {
            int offset = 0;
            Span<char> exTypeNameManaged = exInfoBuffer.AsSpan(0, ComAsyncException.ExceptionTypeNameMaxLength);

            offset += exTypeNameManaged.Length;
            Span<char> exInfoManaged = exInfoBuffer.AsSpan(offset, ComAsyncException.ExceptionInfoMaxLength);

            offset += exInfoManaged.Length;
            Span<char> exMessageManaged = exInfoBuffer.AsSpan(offset, ComAsyncException.ExceptionMessageMaxLength);

            offset += exMessageManaged.Length;
            Span<char> exStackTraceManaged = exInfoBuffer.AsSpan(offset, ComAsyncException.ExceptionStackTraceMaxLength);

            // TODO: Split the exception name from typeName as it might contain additional info for some exception types.
            Encoding.UTF8.TryGetChars(exTypeNameSpan,   exTypeNameManaged,   out int exTypeNameManagedWritten);
            Encoding.UTF8.TryGetChars(exInfoSpan,       exInfoManaged,       out int exInfoManagedWritten);
            Encoding.UTF8.TryGetChars(exMessageSpan,    exMessageManaged,    out int exMessageManagedWritten);
            Encoding.UTF8.TryGetChars(exStackTraceSpan, exStackTraceManaged, out int exStackTraceWritten);

            exTypeNameManaged   = exTypeNameManaged[..exTypeNameManagedWritten];
            exInfoManaged       = exInfoManaged[..exInfoManagedWritten];
            exMessageManaged    = exMessageManaged[..exMessageManagedWritten];
            exStackTraceManaged = exStackTraceManaged[..exStackTraceWritten];

            if (!ExceptionNames.ExceptionCreateDelegateLookup
                    .TryGetValue(exTypeNameManaged, out ExceptionNames.CreateExceptionCallback<Exception>? createExceptionCallback))
            {
                Exception anyException = new Exception();
                anyException.SetExceptionMessage()          = exMessageManaged.ToString();
                anyException.SetExceptionRemoteStackTrace() = exStackTraceManaged.ToString();
                throw new Exception(exMessageManaged.ToString());
            }

            Exception createdException = createExceptionCallback(exInfoManaged, exMessageManaged);
            string?   stackTrace       = exStackTraceWritten == 0 ? null : exStackTraceManaged.ToString();

            createdException.SetExceptionRemoteStackTrace() = stackTrace;
            return createdException;
        }
        finally
        {
            ArrayPool<char>.Shared.Return(exInfoBuffer);
        }
    }
}
