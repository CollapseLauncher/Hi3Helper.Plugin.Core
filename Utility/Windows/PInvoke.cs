using System;
using System.Runtime.InteropServices;
using static System.Environment;
// ReSharper disable InconsistentNaming
// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo
// ReSharper disable GrammarMistakeInComment

namespace Hi3Helper.Plugin.Core.Utility.Windows;

public static partial class PInvoke
{
    [LibraryImport("kernel32.dll", EntryPoint = "SetEvent", SetLastError = true)]
    public static partial int SetEvent(nint hEvent);

    [LibraryImport("kernel32.dll", EntryPoint = "CreateEventW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    public static unsafe partial nint CreateEvent(nint lpEventAttributes, int bManualReset, int bInitialState, string? lpName);

    [LibraryImport("kernel32.dll", EntryPoint = "CloseHandle", SetLastError = true)]
    public static partial int CloseHandle(nint hObject);

    [LibraryImport("shell32.dll", EntryPoint = "SHGetKnownFolderPath", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    public static partial int SHGetKnownFolderPath(in Guid rfid, uint dwFlags, IntPtr hToken, out string? ppszPath);

    /// Reference:
    /// https://github.com/dotnet/runtime/blob/main/src/libraries/Common/src/Interop/Windows/Shell32/Interop.SHGetKnownFolderPath.cs#L269
    public static string GetFolderPath(SpecialFolder folder, SpecialFolderOption option = SpecialFolderOption.None)
    {
        // We're using SHGetKnownFolderPath instead of SHGetFolderPath as SHGetFolderPath is
        // capped at MAX_PATH.
        //
        // Because we validate both of the input enums we shouldn't have to care about CSIDL and flag
        // definitions we haven't mapped. If we remove or loosen the checks we'd have to account
        // for mapping here (this includes tweaking as SHGetFolderPath would do).
        //
        // The only SpecialFolderOption defines we have are equivalent to KnownFolderFlags.

        string folderGuid;
        string? fallbackEnv = null;
        switch (folder)
        {
            // Special-cased values to not use SHGetFolderPath when we have a more direct option available.
            case SpecialFolder.System:
                // This assumes the system directory always exists and thus we don't need to do anything special for any SpecialFolderOption.
                return SystemDirectory;
            default:
                return string.Empty;

            // Map the SpecialFolder to the appropriate Guid
            case SpecialFolder.ApplicationData:
                folderGuid = KnownFoldersGuid.RoamingAppData;
                fallbackEnv = "APPDATA";
                break;
            case SpecialFolder.CommonApplicationData:
                folderGuid = KnownFoldersGuid.ProgramData;
                fallbackEnv = "ProgramData";
                break;
            case SpecialFolder.LocalApplicationData:
                folderGuid = KnownFoldersGuid.LocalAppData;
                fallbackEnv = "LOCALAPPDATA";
                break;
            case SpecialFolder.Cookies:
                folderGuid = KnownFoldersGuid.Cookies;
                break;
            case SpecialFolder.Desktop:
                folderGuid = KnownFoldersGuid.Desktop;
                break;
            case SpecialFolder.Favorites:
                folderGuid = KnownFoldersGuid.Favorites;
                break;
            case SpecialFolder.History:
                folderGuid = KnownFoldersGuid.History;
                break;
            case SpecialFolder.InternetCache:
                folderGuid = KnownFoldersGuid.InternetCache;
                break;
            case SpecialFolder.Programs:
                folderGuid = KnownFoldersGuid.Programs;
                break;
            case SpecialFolder.MyComputer:
                folderGuid = KnownFoldersGuid.ComputerFolder;
                break;
            case SpecialFolder.MyMusic:
                folderGuid = KnownFoldersGuid.Music;
                break;
            case SpecialFolder.MyPictures:
                folderGuid = KnownFoldersGuid.Pictures;
                break;
            case SpecialFolder.MyVideos:
                folderGuid = KnownFoldersGuid.Videos;
                break;
            case SpecialFolder.Recent:
                folderGuid = KnownFoldersGuid.Recent;
                break;
            case SpecialFolder.SendTo:
                folderGuid = KnownFoldersGuid.SendTo;
                break;
            case SpecialFolder.StartMenu:
                folderGuid = KnownFoldersGuid.StartMenu;
                break;
            case SpecialFolder.Startup:
                folderGuid = KnownFoldersGuid.Startup;
                break;
            case SpecialFolder.Templates:
                folderGuid = KnownFoldersGuid.Templates;
                break;
            case SpecialFolder.DesktopDirectory:
                folderGuid = KnownFoldersGuid.Desktop;
                break;
            case SpecialFolder.Personal:
                // Same as Personal
                // case SpecialFolder.MyDocuments:
                folderGuid = KnownFoldersGuid.Documents;
                break;
            case SpecialFolder.ProgramFiles:
                folderGuid = KnownFoldersGuid.ProgramFiles;
                fallbackEnv = "ProgramFiles";
                break;
            case SpecialFolder.CommonProgramFiles:
                folderGuid = KnownFoldersGuid.ProgramFilesCommon;
                fallbackEnv = "CommonProgramFiles";
                break;
            case SpecialFolder.AdminTools:
                folderGuid = KnownFoldersGuid.AdminTools;
                break;
            case SpecialFolder.CDBurning:
                folderGuid = KnownFoldersGuid.CDBurning;
                break;
            case SpecialFolder.CommonAdminTools:
                folderGuid = KnownFoldersGuid.CommonAdminTools;
                break;
            case SpecialFolder.CommonDocuments:
                folderGuid = KnownFoldersGuid.PublicDocuments;
                break;
            case SpecialFolder.CommonMusic:
                folderGuid = KnownFoldersGuid.PublicMusic;
                break;
            case SpecialFolder.CommonOemLinks:
                folderGuid = KnownFoldersGuid.CommonOEMLinks;
                break;
            case SpecialFolder.CommonPictures:
                folderGuid = KnownFoldersGuid.PublicPictures;
                break;
            case SpecialFolder.CommonStartMenu:
                folderGuid = KnownFoldersGuid.CommonStartMenu;
                break;
            case SpecialFolder.CommonPrograms:
                folderGuid = KnownFoldersGuid.CommonPrograms;
                break;
            case SpecialFolder.CommonStartup:
                folderGuid = KnownFoldersGuid.CommonStartup;
                break;
            case SpecialFolder.CommonDesktopDirectory:
                folderGuid = KnownFoldersGuid.PublicDesktop;
                break;
            case SpecialFolder.CommonTemplates:
                folderGuid = KnownFoldersGuid.CommonTemplates;
                break;
            case SpecialFolder.CommonVideos:
                folderGuid = KnownFoldersGuid.PublicVideos;
                break;
            case SpecialFolder.Fonts:
                folderGuid = KnownFoldersGuid.Fonts;
                break;
            case SpecialFolder.NetworkShortcuts:
                folderGuid = KnownFoldersGuid.NetHood;
                break;
            case SpecialFolder.PrinterShortcuts:
                folderGuid = KnownFoldersGuid.PrintersFolder;
                break;
            case SpecialFolder.UserProfile:
                folderGuid = KnownFoldersGuid.Profile;
                fallbackEnv = "USERPROFILE";
                break;
            case SpecialFolder.CommonProgramFilesX86:
                folderGuid = KnownFoldersGuid.ProgramFilesCommonX86;
                fallbackEnv = "CommonProgramFiles(x86)";
                break;
            case SpecialFolder.ProgramFilesX86:
                folderGuid = KnownFoldersGuid.ProgramFilesX86;
                fallbackEnv = "ProgramFiles(x86)";
                break;
            case SpecialFolder.Resources:
                folderGuid = KnownFoldersGuid.ResourceDir;
                break;
            case SpecialFolder.LocalizedResources:
                folderGuid = KnownFoldersGuid.LocalizedResourcesDir;
                break;
            case SpecialFolder.SystemX86:
                folderGuid = KnownFoldersGuid.SystemX86;
                break;
            case SpecialFolder.Windows:
                folderGuid = KnownFoldersGuid.Windows;
                fallbackEnv = "windir";
                break;
        }

        Guid folderId = new Guid(folderGuid);
        int  hr       = SHGetKnownFolderPath(in folderId, (uint)option, IntPtr.Zero, out string? path);
        if (hr == 0 && !string.IsNullOrEmpty(path))
            return path;

        // Fallback logic if SHGetKnownFolderPath failed (nanoserver)
        return fallbackEnv != null ? GetEnvironmentVariable(fallbackEnv) ?? string.Empty : string.Empty;
    }
}
