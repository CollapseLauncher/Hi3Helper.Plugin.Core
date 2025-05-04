using System.Runtime.InteropServices;

namespace Hi3Helper.Plugin.Core.Management
{
    public interface IVersion
    {
        int get_Major();
        int get_Minor();
        int get_Patch();
        int get_Build();

        [return: MarshalAs(UnmanagedType.LPWStr)]
        string? ToString();
    }
}
