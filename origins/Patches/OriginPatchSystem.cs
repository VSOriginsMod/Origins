using Origins.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace Origins.Patches
{
    internal class OriginPatchSystem : ModSystem
    {
        /// <summary>
        /// For conveniently iterating over all classes implementing ICodePatch in order to preform code patches.
        /// </summary>
        private static readonly IEnumerable<Type> patchTypes = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(t => null != t.GetInterface("ICodePatch"));


        private ICoreAPI api;
        private ICoreServerAPI sapi;

        public override void Start(ICoreAPI api)
        {
            this.api = api;

            OriginsLogger.Debug(api, "[OriginPatchSystem] Registering " + patchTypes.ToList().Count + " patches");

            foreach (var patch in patchTypes)
            {
                try
                {
                    OriginsLogger.Debug(api, "Attempting to register following code patch: " + patch.Name);
                    patch
                        .GetMethod("RegisterPatch", new[] { typeof(ICoreAPI) })
                        .Invoke(null, new object[] { api });
                }
                catch (Exception err)
                {
                    OriginsLogger.Debug(api, "Oops! " + patch.Name + " was not recognized by this process... Does it implement Origins.Patches.IPatch");
                    OriginsLogger.Error(api, err);
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

            foreach (var patch in patchTypes)
            {
                try
                {
                    OriginsLogger.Debug(api, "Attempting to apply following code patch: " + patch.Name);
                    patch
                        .GetMethod("ApplyPatch", new[] { typeof(ICoreAPI) })
                        .Invoke(null, new object[] { api });
                }
                catch (Exception err)
                {
                    OriginsLogger.Debug(api, "Oops! " + patch.Name + " was not recognized by this process... Does it implement Origins.Patches.IPatch");
                    OriginsLogger.Error(api, err);
                }

            }
        }
    }
}
