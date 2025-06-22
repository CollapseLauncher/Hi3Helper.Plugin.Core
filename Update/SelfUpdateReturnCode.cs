using System;

namespace Hi3Helper.Plugin.Core.Update;

[Flags]
public enum SelfUpdateReturnCode : uint
{
    // Base
    Ok    = 0b_00000000_00000000_00000000_00000001,
    Error = 0b_00000000_00000000_00000000_00000010,

    // Top-level: Error
    NetworkError = Error | 0b_00000000_00000000_00000000_00000100,
    LocalError   = Error | 0b_00000000_00000000_00000000_00001000,
    UnknownError = Error | 0b_00000000_00000000_00000000_00010000,

    // NetworkError
    NoReachableCdn     = NetworkError | 0b_00000000_00000000_00000001_00000000,
    UpdateFileNotFound = NetworkError | 0b_00000000_00000000_00000010_00000000,
    CdnInternalError   = NetworkError | 0b_00000000_00000000_00000100_00000000,
    JsonParsingError   = NetworkError | 0b_00000000_00000000_00001000_00000000,
    UpdateCancelled    = NetworkError | 0b_00000000_00000000_00010000_00000000,

    // LocalError
    FileLocked        = LocalError | 0b_00000000_00000001_00000000_00000000,
    DiskAccessDenied  = LocalError | 0b_00000000_00000010_00000000_00000000,
    DiskFull          = LocalError | 0b_00000000_00000100_00000000_00000000,
    PathNotFound      = LocalError | 0b_00000000_00001000_00000000_00000000,
    DriveNotReady     = LocalError | 0b_00000000_00010000_00000000_00000000,
    FileAlreadyExists = LocalError | 0b_00000000_00100000_00000000_00000000,
    NameTooLong       = LocalError | 0b_00000000_01000000_00000000_00000000,

    // Top-level: Ok
    NoAvailableUpdate  = Ok | 0b_00000001_00000000_00000000_00000000,
    RollingBackSuccess = Ok | 0b_00000010_00000000_00000000_00000000,
    UpdateSuccess      = Ok | 0b_00000100_00000000_00000000_00000000,
    UpdateIsAvailable  = Ok | 0b_00001000_00000000_00000000_00000000
}