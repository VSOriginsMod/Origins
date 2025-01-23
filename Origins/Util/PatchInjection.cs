using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Vintagestory.API.Common;

namespace Origins.Util;

interface IPatch
{
    public static abstract void RegisterPatch(ICoreAPI api);
}

interface ICodePatch : IPatch
{
    public static abstract void ApplyPatch(ICoreAPI api);
}

internal class PatchInjection : ModSystem
{
    /// <summary>
    /// For conveniently iterating over all classes implementing IPatch.
    /// </summary>
    private static readonly IEnumerable<Type> patches = Assembly
        .GetExecutingAssembly()
        .GetTypes()
        .Where(
            t => !t.IsInterface && t
                .GetInterfaces()
                .Intersect(new Type[2] { typeof(IPatch), typeof(ICodePatch) })
                .Any(y => null != y.GetInterface("IPatch"))
        );

    public override void Start(ICoreAPI api)
    {
        OriginsLogger.Debug(api, "[PatchInjection] Registering {0} patches", patches.ToList().Count);

        foreach (var patch in patches)
        {
            try
            {
                OriginsLogger.Debug(api, "Attempting to register following {0}patch: {1}",
                    null != patch.GetInterface("ICodePatch") ? "code " : "",
                    patch.Name
                );

                patch
                    .GetMethod("RegisterPatch", new[] { typeof(ICoreAPI) })
                    .Invoke(patch, new object[] { api });
            }
            catch (Exception err)
            {

                OriginsLogger.Error(api,
                    "Oops! {0} was not recognized by this process... Does it implement Origins.Patches.IPatch?\n{1}",
                    patch.Name,
                    err
                );
            }

        }
    }

    public override void AssetsFinalize(ICoreAPI api)
    {
        // NOTE(chris): if this conditional is ever changed update docs in ICodePatch
        // only want to patch on server-side
        if (EnumAppSide.Server != api.Side)
        {
            return;
        }

        foreach (var patch in patches.Where(t => null != t.GetInterface("ICodePatch")))
        {
            try
            {
                OriginsLogger.Debug(api, "Attempting to apply following code patch: {0}",
                    patch.Name
                );

                patch
                    .GetMethod("ApplyPatch", new[] { typeof(ICoreAPI) })
                    .Invoke(patch, new object[] { api });
            }
            catch (Exception err)
            {
                OriginsLogger.Debug(api,
                    "Oops! {0} was not recognized by this process... Does it implement Origins.Patches.ICodePatch?\n{1}",
                    patch.Name,
                    err
                );
            }

        }
    }
}
