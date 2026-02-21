using Hi3Helper.Plugin.Core.Management;

namespace Hi3Helper.Plugin.Core;

/// <summary>
/// Inherited <see cref="SharedStatic"/> with additional supports for API extensions which require call or property access to derived exports.
/// </summary>
public partial class SharedStaticV1Ext<T> : SharedStaticV1Ext
    where T : SharedStaticV1Ext<T>, new()
{
    private static T ThisExtensionExport { get; }

    static SharedStaticV1Ext()
    {
        ThisExtensionExport = new T();

        /* ----------------------------------------------------------------------
         * Plugin extension exports
         * ----------------------------------------------------------------------
         * These exports are optional and can be removed if it's not necessarily
         * used. These optional exports are included under additional
         * functionalities used as a subset of v0.1 API standard.
         */
        InitExtension_Update1Exports();
        InitExtension_Update2Exports();
        InitExtension_Update3Exports();
        InitExtension_Update4Exports();
    }

    /// <summary>
    /// Specify which <see cref="IPlugin"/> instance to load and use in this plugin.
    /// </summary>
    /// <typeparam name="TPlugin">A member of COM Interface of <see cref="IPlugin"/>.</typeparam>
    protected new static void Load<TPlugin>(GameVersion interceptDllVersionTo = default)
        where TPlugin : class, IPlugin, new()
        => SharedStatic.Load<TPlugin>(interceptDllVersionTo);
}
