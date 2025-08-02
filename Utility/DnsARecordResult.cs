// ReSharper disable IdentifierTypo

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Hi3Helper.Plugin.Core.Utility;

/// <summary>
/// Unmanaged struct which contains a string which represents both IPv4 or IPv6.
/// </summary>
public unsafe struct DnsARecordResult
{
    /// <summary>
    /// The version of the IP string. The value must be either 4 (for IPv4) or 6 (for IPv6).
    /// </summary>
    public int Version;

    /// <summary>
    /// Pointer to a string represents the IP address.
    /// </summary>
    public char* AddressString;

    /// <summary>
    /// Next entry of the record result. This can be null if no any entries left.
    /// </summary>
    public DnsARecordResult* NextResult;

    /// <summary>
    /// Gets an array of <see cref="IPAddress"/> and free the struct data from the pointer.
    /// </summary>
    /// <remarks>
    /// If the <param name="dnsARecordP"/> is returning null or zero pointer, the method will return an empty array instead.
    /// </remarks>
    /// <param name="dnsARecordP">A pointer to <see cref="DnsARecordResult"/> struct.</param>
    /// <returns>An array of <see cref="IPAddress"/></returns>
    // ReSharper disable once InconsistentNaming
    public static IPAddress[] GetIPAddressArray(nint dnsARecordP)
        => GetIPAddressArray(dnsARecordP.AsPointer<DnsARecordResult>());

    /// <summary>
    /// Gets an array of <see cref="IPAddress"/> and free the struct data from the pointer.
    /// </summary>
    /// <remarks>
    /// If the <param name="dnsARecordP"/> is returning null or zero pointer, the method will return an empty array instead.
    /// </remarks>
    /// <param name="dnsARecordP">A pointer to <see cref="DnsARecordResult"/> struct.</param>
    /// <returns>An array of <see cref="IPAddress"/></returns>
    // ReSharper disable once InconsistentNaming
    public static IPAddress[] GetIPAddressArray(DnsARecordResult* dnsARecordP)
    {
        // Cast to DnsARecordResult pointer.
        DnsARecordResult* endResultP = dnsARecordP;
        int               count      = 0;

        // Return empty array if the pointer is null.
        if (endResultP == null)
        {
            return [];
        }

        // Start counting to get the length of IPAddress[]
        do
        {
            ++count;
            endResultP = endResultP->NextResult;
        } while (endResultP != null);

        // Allocate the array and reassign the pointer to start converting.
        IPAddress[] returnIpAddresses = GC.AllocateUninitializedArray<IPAddress>(count);
        endResultP = dnsARecordP;

        int offset = 0;
        do
        {
            // Temporarily assign to the next entry.
            DnsARecordResult* next = endResultP->NextResult;

            // Convert the struct into IPAddress instance.
            returnIpAddresses[offset++] = GetIPAddressFromResultAndDispose(endResultP);
            endResultP                  = next;
        } while (endResultP != null); // Do the loop if the next entry is not null.

        // If done, return the array.
        return returnIpAddresses;
    }

    // ReSharper disable once InconsistentNaming
    private static IPAddress GetIPAddressFromResultAndDispose(DnsARecordResult* resultP)
    {
        try
        {
            // Get the span representing the string.
            ReadOnlySpan<char> addressStringSpan = Mem.CreateSpanFromNullTerminated<char>(resultP->AddressString);

            // Try to parse it. Output as IPAddress. Throw if the string is malformed.
            if (!IPAddress.TryParse(addressStringSpan, out IPAddress? value))
            {
                throw new InvalidDataException($"IP Address string is not valid! {addressStringSpan}");
            }

            return value;
        }
        finally
        {
            // If the result is not null, free the string and the struct.
            if (resultP != null)
            {
                Marshal.FreeCoTaskMem((nint)resultP->AddressString); // Equivalent to Utf16StringMarshaller.Free
                Mem.Free(resultP);
            }
        }
    }

    /// <summary>
    /// Creates a single or multiple entries of <see cref="DnsARecordResult"/> from an array of <see cref="IPAddress"/>.
    /// </summary>
    /// <remarks>
    /// The field of <see cref="DnsARecordResult.NextResult"/> will contain a pointer of the next <see cref="DnsARecordResult"/> entry if the array of <see cref="IPAddress"/> is used.<br/>
    /// When the array is empty, a null pointer will be returned instead.
    /// </remarks>
    /// <param name="addressSpan">A span/array of addresses to be converted.</param>
    /// <returns>A pointer to a struct of <see cref="DnsARecordResult"/></returns>
    public static nint CreateToIntPtr(params ReadOnlySpan<IPAddress> addressSpan)
        => (nint)Create(addressSpan);

    /// <summary>
    /// Creates a single or multiple entries of <see cref="DnsARecordResult"/> from an array of <see cref="IPAddress"/>.
    /// </summary>
    /// <remarks>
    /// The field of <see cref="DnsARecordResult.NextResult"/> will contain a pointer of the next <see cref="DnsARecordResult"/> entry if the array of <see cref="IPAddress"/> is used.<br/>
    /// When the array is empty, a null pointer will be returned instead.
    /// </remarks>
    /// <param name="addressSpan">A span/array of addresses to be converted.</param>
    /// <returns>A pointer to a struct of <see cref="DnsARecordResult"/></returns>
    public static DnsARecordResult* Create(params ReadOnlySpan<IPAddress> addressSpan)
    {
        if (addressSpan.IsEmpty)
        {
            return null;
        }

        int               offset = 0;
        DnsARecordResult* result = null;
        DnsARecordResult* last   = null;
        do
        {
            // Allocate the current entry.
            // It's important to zero-ing the next result pointer first as the memory allocated aren't zeroed.
            DnsARecordResult* current = Mem.Alloc<DnsARecordResult>(1, false);
            current->NextResult = null;

            // If the last entry was allocated before, assign the current entry as its next ones.
            if (last != null)
            {
                last->NextResult = current;
            }

            // Assign result if it's null.
            if (result == null)
            {
                result = current;
            }

            // Get the current IPAddress instance, convert it to string and determine the version.
            IPAddress address         = addressSpan[offset++];
            string    ipAddressString = address.ToString();
            int       version         = address.AddressFamily == AddressFamily.InterNetworkV6 ? 6 : 4;

            // Allocate the string, copy it into native memory and set the version
            char* stringP = (char*)Utf16StringMarshaller.ConvertToUnmanaged(ipAddressString);
            current->Version       = version;
            current->AddressString = stringP;

            last = current;
        } while (offset < addressSpan.Length); // Repeat the process if there's still another entry

        // Return the result
        return result;
    }
}
