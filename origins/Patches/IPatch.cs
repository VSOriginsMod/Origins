using Vintagestory.API.Common;

namespace Origins.Patches
{
  interface IPatch
  {
    public static abstract void RegisterPatch(ICoreAPI api);
    public static abstract void ApplyPatch(ICoreAPI api);
  }
}