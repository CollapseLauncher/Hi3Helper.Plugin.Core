// ReSharper disable IdentifierTypo

using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.Marshalling;

namespace Hi3Helper.Plugin.Core.Utility;

public unsafe struct DnsARecordResult
{
    public int               Version;
    public char*             AddressString;
    public DnsARecordResult* NextResult;

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
            DnsARecordResult* current = Mem.Alloc<DnsARecordResult>(1, false);
            current->NextResult = null;

            if (last != null)
            {
                last->NextResult = current;
            }

            if (result == null)
            {
                result = current;
            }

            IPAddress address = addressSpan[offset++];

            string ipAddressString = address.ToString();
            int    version         = address.AddressFamily == AddressFamily.InterNetworkV6 ? 6 : 4;

            char* stringP = (char*)Utf16StringMarshaller.ConvertToUnmanaged(ipAddressString);
            current->Version       = version;
            current->AddressString = stringP;

            last = current;
        } while (offset < addressSpan.Length);

        return result;
    }
}
