using Vintagestory.API.Common;

namespace Origins.Patches
{
    interface IPatch
    {
        public static abstract void RegisterPatch(ICoreAPI api);
    }

    interface ICodePatch : IPatch
    {
        public static abstract void ApplyPatch(ICoreAPI api);
    }
}
