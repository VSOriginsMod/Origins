using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace rpskills
{
    /// <summary>
    /// ModSystem is the base for any VintageStory code mods.
    /// </summary>
    /// HarmonyPatch is required for any class that patches
    [HarmonyPatch]
    public class RPSkillsSystem : ModSystem
    {

        const string MOD_NAME = "rpskills";

        // NOTE(Chris): can all of the ICoreAPI variables be unionized in a record?
        // record API {
        //     record ServerAPI() : API();
        //     record CommonAPI() : API();
        //     record ClientAPI() : API();

        //     private API() {}
        // }

        /// <summary>
        /// Utility for accessing common client/server functionality.
        /// </summary>
        ICoreAPI api;

        Harmony harmony;

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            this.api = api;

            harmony = new Harmony(MOD_NAME);

        }

        public override void Dispose() {
            base.Dispose();
            harmony.UnpatchAll(MOD_NAME);
        }

    }
}
