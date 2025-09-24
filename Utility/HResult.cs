using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Hi3Helper.Plugin.Core.Utility
{
    public readonly struct HResult
            : IEquatable<HResult>
    {
        internal readonly int Value;

        internal HResult(int                         value) => Value = value;
        public static implicit operator int(HResult  value) => value.Value;
        public static implicit operator uint(HResult value) => (uint)value.Value;
        public static implicit operator HResult(int  value) => new(value);
        public static implicit operator HResult(uint value) => new((int)value);
        public static implicit operator HResult(bool value) => new(value ? 0 : 1);

        public static readonly HResult Ok    = new(0);
        public static readonly HResult False = new(1);

        public static bool operator ==(HResult left, HResult right) => left.Value == right.Value;
        public static bool operator !=(HResult left, HResult right) => !(left == right);
        public bool Equals(HResult other) => Value == other.Value;
        public override bool Equals(object? obj) => obj is HResult other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => string.Format(CultureInfo.InvariantCulture, "0x{0:X8}", Value);

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal bool Succeeded => Value >= 0;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal bool Failed => Value < 0;

        /// <inheritdoc cref="Marshal.ThrowExceptionForHR(int, nint)" />
        /// <param name="errorInfo">
        /// A pointer to the IErrorInfo interface that provides more information about the
        /// error. You can specify <see cref="nint.Zero"/> to use the current IErrorInfo interface, or
        /// <c>new nint(-1)</c> to ignore the current IErrorInfo interface and construct the exception
        /// just from the error code.
        /// </param>
        /// <returns><see langword="this"/><see cref="HResult"/>, if it does not reflect an error.</returns>
        /// <seealso cref="Marshal.ThrowExceptionForHR(int, nint)"/>
        public HResult ThrowOnFailure(nint errorInfo = 0)
        {
            Marshal.ThrowExceptionForHR(Value, errorInfo);
            return this;
        }

        public string ToString(string format, IFormatProvider formatProvider) => ((uint)Value).ToString(format, formatProvider);
    }
}
