using System;
using System.Buffers;
using System.Collections.Generic;
using System.Data;
using System.IO;
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

    private static partial class ExceptionNames
    {
        private static readonly Dictionary<string, CreateExceptionCallback<Exception>> ExceptionCreateDelegate = new(StringComparer.OrdinalIgnoreCase)
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
            { ExNameIndexOutOfRangeException, CreateAnyException<IndexOutOfRangeException> },
            { ExNameInsufficientExecutionStackException, CreateAnyException<InsufficientExecutionStackException> },
            { ExNameInsufficientMemoryException, CreateAnyException<InsufficientMemoryException> },
            { ExNameInvalidCastException, CreateAnyException<InvalidCastException> },
            { ExNameInvalidOperationException, CreateAnyException<InvalidOperationException> },
            { ExNameInvalidProgramException, CreateAnyException<InvalidProgramException> },
            { ExNameIOException, CreateAnyException<IOException> },
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

        internal static readonly Dictionary<string, CreateExceptionCallback<Exception>>.AlternateLookup<ReadOnlySpan<char>> ExceptionCreateDelegateLookup = ExceptionCreateDelegate.GetAlternateLookup<ReadOnlySpan<char>>();

        internal delegate T CreateExceptionCallback<out T>(Span<char> typeNameInfo, Span<char> message) where T : Exception, new();

        private static ObjectDisposedException CreateObjectDisposedException(Span<char> typeNameInfo, Span<char> message)
        {
            string exceptionMessage = message.ToString();
            Exception? innerException = null;
            ObjectDisposedException exception = new ObjectDisposedException(exceptionMessage, innerException);
            return exception;
        }

        private static TypeInitializationException CreateTypeInitializationException(Span<char> typeNameInfo, Span<char> message)
        {
            // TODO: Pass the type name to the typeNameInfo
            TypeInitializationException exception = new TypeInitializationException(null, null);

            return exception;
        }

        private static Exception CreateAnyException<T>(Span<char> typeNameInfo, Span<char> message)
            where T : Exception, new()
        {
            Exception exception = new T();
            exception.SetExceptionMessage() = message.ToString();

            return exception;
        }
    }

    private static unsafe void ThrowExceptionFromInfo(ComAsyncResult* result)
    {
        byte* exTypeName = result->ExceptionTypeByName;
        byte* exMessage  = result->ExceptionMessage;

        ReadOnlySpan<byte> exTypeNameSpan = MemoryMarshal.CreateReadOnlySpanFromNullTerminated(exTypeName);
        ReadOnlySpan<byte> exMessageSpan  = MemoryMarshal.CreateReadOnlySpanFromNullTerminated(exMessage);

        char[] exInfoBuffer = ArrayPool<char>.Shared.Rent(ComAsyncResult.ExceptionTypeNameMaxLength + ComAsyncResult.ExceptionMessageMaxLength);
        try
        {
            Span<char> exTypeNameManaged = exInfoBuffer.AsSpan(0, ComAsyncResult.ExceptionTypeNameMaxLength);
            Span<char> exMessageManaged  = exInfoBuffer.AsSpan(ComAsyncResult.ExceptionTypeNameMaxLength, ComAsyncResult.ExceptionMessageMaxLength);

            // TODO: Split the exception name from typeName as it might contain additional info for some exception types.
            Encoding.UTF8.TryGetChars(exTypeNameSpan, exTypeNameManaged, out int exTypeNameManagedWritten);
            Encoding.UTF8.TryGetChars(exMessageSpan,  exMessageManaged,  out int exMessageManagedWritten);

            exTypeNameManaged = exTypeNameManaged[..exTypeNameManagedWritten];
            exMessageManaged  = exMessageManaged[..exMessageManagedWritten];

            if (!ExceptionNames.ExceptionCreateDelegateLookup
                .TryGetValue(exTypeNameManaged, out ExceptionNames.CreateExceptionCallback<Exception>? createExceptionCallback))
            {
                throw new Exception(exMessageManaged.ToString());
            }

            throw createExceptionCallback(exTypeNameManaged, exMessageManaged);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(exInfoBuffer);
        }
    }
}
