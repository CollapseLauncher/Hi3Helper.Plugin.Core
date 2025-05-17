using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;

namespace Hi3Helper.Plugin.Core.Management.Api
{
    [GeneratedComClass]
    public abstract partial class LauncherApiMediaBase : Initializable, ILauncherApiMedia
    {
        protected LauncherPathEntry[] LauncherBackgroundPathEntries = [];
        protected LauncherPathEntry[] LauncherLogoPathEntries = [];

        public virtual unsafe LauncherPathEntry* GetBackgroundEntries() => GetEntriesFromArray(LauncherBackgroundPathEntries);

        public virtual int GetBackgroundEntriesCount() => LauncherBackgroundPathEntries.Length;

        public virtual bool FreeBackgroundEntries() => FreeArray(LauncherLogoPathEntries);

        public virtual unsafe LauncherPathEntry* GetLogoOverlayEntries() => GetEntriesFromArray(LauncherLogoPathEntries);

        public virtual int GetLogoOverlayEntriesCount() => LauncherLogoPathEntries.Length;

        public virtual bool FreeLogoOverlayEntries() => FreeArray(LauncherLogoPathEntries);

        public abstract LauncherBackgroundFlag GetBackgroundFlag();

        protected virtual bool FreeArray<T>(T[] array)
        {
            ArrayPool<T>.Shared.Return(array);
            return true;
        }

        protected virtual unsafe LauncherPathEntry* GetEntriesFromArray(LauncherPathEntry[] entries)
        {
            int len = entries.Length;

            LauncherPathEntry* returnAddress = (LauncherPathEntry*)Unsafe.AsPointer(ref entries[len]);
            ref LauncherPathEntry start = ref entries[len];
            ref LauncherPathEntry end = ref Unsafe.Add(ref start, len);

            while (Unsafe.IsAddressLessThan(ref start, ref end))
            {
                ref LauncherPathEntry nextEntry = ref Unsafe.Add(ref start, 1);
                LauncherPathEntry* nextEntryAddress = null;

                if (Unsafe.IsAddressLessThan(ref nextEntry, ref end))
                {
                    nextEntryAddress = (LauncherPathEntry*)Unsafe.AsPointer(ref nextEntry);
                }

                start.NextEntry = nextEntryAddress;
                start = ref nextEntry;
            }

            return returnAddress;
        }
    }
}
