using Origins.Gui;
using Origins.Systems;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;

namespace Origins.Character
{
    internal class SkillSystem : ModSystem
    {
        // TODO(chris): fix backwards logic
        public static List<Skill> Elements;
        public List<Skill> Skills;

        private GuiDialogSkills PlayerSkillsUI;

        public override double ExecuteOrder()
        {
            return base.ExecuteOrder();
        }

        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return true;
        }

        public override void Start(ICoreAPI api)
        {
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            //Note(Moon):
            //these lines are what's needed in order to turn the dialog box, the initialization
            //of the GuiDialogSkills can be moved to a seprate class and likely will be at a later date.
            //It just needs the capi in order to be be hooked for the hotkey.
            PlayerSkillsUI = new GuiDialogSkills(api);
            api.Input.RegisterHotKey(
                "Skill Interface",
                "Opens up the Skills GUI",
                GlKeys.O,
                HotkeyType.GUIOrOtherControls);

            api.Input.SetHotKeyHandler("Skill Interface", ToggleGUI);
        }

        public override void AssetsLoaded(ICoreAPI api)
        {
            Skills = api.Assets.Get("origins:config/skills.json").ToObject<List<Skill>>(null);
            Elements = Skills;
            OriginsLogger.Debug(api, "Skills loaded");
        }

        public bool ToggleGUI(KeyCombination comb)
        {
            if (PlayerSkillsUI.IsOpened())
                PlayerSkillsUI.TryClose();
            else
                PlayerSkillsUI.TryOpen();

            return true;
        }

        // [Obsolete("This function should be removed because it relied on a previous logical model")]
        public void InitializePlayer(IServerPlayer player)
        {
            foreach (Skill skill in Skills)
            {
                player.WorldData.EntityPlayer.WatchedAttributes.RemoveAttribute("s_" + skill.Name);
                player.WorldData.EntityPlayer.WatchedAttributes.SetFloat("s_" + skill.Name, skill.Level);
            }

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
        public static void SetSkill(IServerPlayer player, string SkillName, float val)
        {
            player.Entity.WatchedAttributes.SetFloat(SkillName, val);
        }

        public static void SetSkill(ICoreServerAPI api, string playerUid, string SkillName, float val)
        {
            SetSkill(
                api.World.PlayerByUid(playerUid) as IServerPlayer,
                SkillName, val);
        }

        public static float GetSkill(IServerPlayer player, string SkillName)
        {
            return player.Entity.WatchedAttributes.GetFloat(SkillName);
        }

        public static float GetSkill(ICoreServerAPI api, string playerUid, string SkillName)
        {
            return GetSkill(
                api.World.PlayerByUid(playerUid) as IServerPlayer,
                SkillName);
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


    }

    public class SkillSet : ArrayAttribute<Skill>
    {

    }

    // TODO (chris): implement IAttribute when migrating to using IAssetManager Assets
    public class Skill //: IAttribute
    {
        public string Name;

        public int Level;

    }
}
