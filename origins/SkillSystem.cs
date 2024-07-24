using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;
using Vintagestory.Client.NoObf;

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

        /// <summary>
        /// Set player's skill level
        /// </summary>
        /// 
        /// <remarks>
        /// This must be preformed server-side
        /// </remarks>
        /// 
        /// <param name="player"></param>
        /// <param name="SkillName"></param>
        /// <param name="val"></param>
        public static void SetSkil(IPlayer player, string SkillName, IAttribute val)
        {
            player.Entity.WatchedAttributes.SetAttribute(SkillName, val);
        }

        public static void SetSkil(ICoreServerAPI api, string playerUid, string SkillName, IAttribute val)
        {
            api.World.PlayerByUid(playerUid).Entity.WatchedAttributes.SetAttribute(SkillName, val);
        }

        /// <summary>
        /// Increment skill by 1
        /// </summary>
        /// 
        /// <remarks>
        /// This must be called server side!
        /// </remarks>
        /// 
        /// <param name="player"></param>
        /// <param name="SkillName"></param>
        /// <returns></returns>
        public static float IncrementSkill(IPlayer player, string SkillName)
        {
            SkillName = "s_" + SkillName;
            var skillExp = player.Entity.WatchedAttributes.GetFloat(SkillName);
            skillExp += 1;
            player.Entity.WatchedAttributes.SetFloat(SkillName, skillExp);
            return skillExp;
        }

        public static void InitializePlayer(IServerPlayer player)
        {
            foreach (Skill skill in Elements)
            {
                player.WorldData.EntityPlayer.WatchedAttributes.RemoveAttribute("s_" + skill.Name);
                player.WorldData.EntityPlayer.WatchedAttributes.SetFloat("s_" + skill.Name, skill.Level);
            }

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
