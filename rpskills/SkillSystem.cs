using rpskills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace origins
{
    internal class SkillSystem : ProgressionSystem<Skill>
    {
        /*
         * 
         * jasdfasipodfpoiadsripoaewuiporuqwipoerpalskjd;flkjadsfipoewpior
         * 
         * 
         * STATIC! STATIC!!!
         */
        internal static void Build(ICoreAPI api)
        {
            ProgressionSystem<Skill>.Load(api, "origins:config/skills.json");

            api.Logger.Event("loaded skills");
        }

        public static Skill GetSkill(string name)
        {
            return ElementsByName[name];
        }


        private PlayerSkillsUI PlayerSkillsUI;

        public SkillSystem(ICoreAPI api) : base(api)
        {
        }

        internal override void ClientInit(ICoreClientAPI capi)
        {
            //Note(Moon):
            //these lines are what's needed in order to turn the dialog box, the initialization
            //of the PlayerSkillsUI can be moved to a seprate class and likely will be at a later date.
            //It just needs the capi in order to be be hooked for the hotkey.
            PlayerSkillsUI = new PlayerSkillsUI(capi);
            capi.Input.RegisterHotKey("Skill Interface", "Opens up the Skills GUI", GlKeys.O, HotkeyType.GUIOrOtherControls);
            capi.Input.SetHotKeyHandler("Skill Interface", ToggleGUI);
        }

        internal override void ServerInit(ICoreServerAPI sapi)
        {
            throw new NotImplementedException();
        }

        public bool ToggleGUI(KeyCombination comb)
        {
            if (PlayerSkillsUI.IsOpened())
                PlayerSkillsUI.TryClose();
            else
                PlayerSkillsUI.TryOpen();

            return true;
        }
    }

    public class Skill : INamedProgression
    {
        public string Name;

        public int Level;

        public List<string> Paths;

        string INamedProgression.Name => Name;
    }
}
