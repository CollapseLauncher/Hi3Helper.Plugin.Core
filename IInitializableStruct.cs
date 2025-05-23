// ReSharper disable IdentifierTypo

namespace Hi3Helper.Plugin.Core;

/// <summary>
/// An interface that defines the initialization of the struct number.
/// This is used for the struct that needs to be initialized before use.
/// </summary>
public interface IInitializableStruct
{
    /// <summary>
    /// Initialize the inner fields of the struct before use.
    /// </summary>
    void InitInner();
}
