using HarmonyLib;
using Vintagestory.API.Client;
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
        private const string MOD_NAME = "rpskills";

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
        private ICoreAPI api;

        private ICoreClientAPI capi;
        private Harmony harmony;

        private PlayerSkillsUI playerSkillsUI;

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            this.api = api;

            harmony = new Harmony(MOD_NAME);
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);
            capi = api;

            //Note(Moon):
            //these lines are what's needed in order to turn the dialog box, the initialization
            //of the PlayerSkillsUI can be moved to a seprate class and likely will be at a later date.
            //It just needs the capi in order to be be hooked for the hotkey.
            playerSkillsUI = new PlayerSkillsUI(capi);
            capi.Input.RegisterHotKey("Skill Interface", "Opens up the Skills GUI", GlKeys.O, HotkeyType.GUIOrOtherControls);
            capi.Input.SetHotKeyHandler("Skill Interface", ToggleGUI);
        }

        public bool ToggleGUI(KeyCombination comb)
        {
            if (playerSkillsUI.IsOpened())
                playerSkillsUI.TryClose();
            else
                playerSkillsUI.TryOpen();

            return true;
        }

        public override void Dispose()
        {
            base.Dispose();
            harmony.UnpatchAll(MOD_NAME);
        }
    }
}