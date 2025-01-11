using Vintagestory.API.Common;

namespace Origins.Patches
{
    interface ICodePatch
    {
        public static abstract void RegisterPatch(ICoreAPI api);

        /// <summary>
        /// This is assumed to execute only on server-side in OriginPatchSystem.AssetsFinalize
        /// </summary>
        /// <param name="api"></param>
        public static abstract void ApplyPatch(ICoreAPI api);
    }
}
