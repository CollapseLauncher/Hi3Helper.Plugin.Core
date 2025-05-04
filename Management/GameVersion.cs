using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hi3Helper.Plugin.Core.Management
{
    public readonly struct GameVersion(int major, int minor, int patch, int build)
        : IVersion
    {
        private readonly int _major = major;
        private readonly int _minor = minor;
        private readonly int _patch = patch;
        private readonly int _build = build;

        public int Major { get; } = major;
        int IVersion.get_Major() => Major;

        public int Minor { get; } = minor;
        int IVersion.get_Minor() => Minor;

        public int Patch { get; } = patch;
        int IVersion.get_Patch() => Patch;

        public int Build { get; } = build;
        int IVersion.get_Build() => Build;

        public override string ToString() => $"{Major}.{Minor}.{Patch}.{Build}";
    }
}
