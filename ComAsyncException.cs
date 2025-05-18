using Hi3Helper.Plugin.Core.Utility;
using System;

namespace Hi3Helper.Plugin.Core
{
    public unsafe struct ComAsyncException
    {
        public const int  ExceptionTypeNameMaxLength   = 64;
        public const int  ExceptionInfoMaxLength       = 64;
        public const int  ExceptionMessageMaxLength    = 384;
        public const int  ExceptionStackTraceMaxLength = 3568;
        public const char ExceptionInfoSeparator       = '$';

        public nint PreviousExceptionHandle;
        public nint NextExceptionHandle;

        // TODO: Marshal exception directly instead of determining it by the name (since it's SUPER SLOW!!!).
        public fixed byte ExceptionTypeByName [ExceptionTypeNameMaxLength];
        public fixed byte ExceptionInfo       [ExceptionInfoMaxLength];
        public fixed byte ExceptionMessage    [ExceptionMessageMaxLength];
        public fixed byte ExceptionStackTrace [ExceptionStackTraceMaxLength];

        public static Exception? GetExceptionFromHandle(nint handle)
        {
            if (handle == nint.Zero)
            {
                return null;
            }

            ComAsyncException* exceptionHandle = (ComAsyncException*)handle;
            Exception?         parentException = ComAsyncExtension.GetExceptionFromInfo(exceptionHandle);

            if (exceptionHandle->NextExceptionHandle == nint.Zero)
            {
                return parentException;
            }

            if (parentException == null)
            {
                return null;
            }

            ComAsyncException* innerExceptionHandle = (ComAsyncException*)exceptionHandle->NextExceptionHandle;
            Exception?         innerException       = null;
            Exception?         lastInnerException   = null;

            while (innerExceptionHandle != null)
            {
                Exception? innerExceptionCurrent = ComAsyncExtension.GetExceptionFromInfo(innerExceptionHandle);
                if (innerExceptionCurrent == null)
                {
                    return parentException;
                }

                if (lastInnerException != null)
                {
                    lastInnerException.SetExceptionInnerException() = innerExceptionCurrent;
                }

                innerException       ??= innerExceptionCurrent;
                lastInnerException   =   innerExceptionCurrent;
                innerExceptionHandle =   (ComAsyncException*)innerExceptionHandle->NextExceptionHandle;
            }

            if (innerException != null)
            {
                parentException.SetExceptionInnerException() = innerException;
            }

            return parentException;
        }
    }
}
