using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vintagestory.Common;
using Vintagestory;
using rpskills.CoreSys;

// TODO(chris): delete all FEAT(chris) annotations before merging
// NOTE(chris): all current WARN(chris) in this file indicates client-server
//              interactions. They depend on the network channel feature, which
//              must be created and debugged first.

namespace rpskills
{
    /// <summary>
    /// ModSystem is the base for any VintageStory code mods.
    /// </summary>
    /// HarmonyPatch is required for any class that patches
    [HarmonyPatch]
    public class CoreSystem : ModSystem
    {

        /*
        ::::::::::::::::::::::::::::::::
        ::::::::::::Constant::::::::::::
        ::::::::::::::::::::::::::::::::
         */

        const string MOD_NAME = "rpskills";


        /*
        ::::::::::::::::::::::::::::::::
        :::::::::::::Shared:::::::::::::
        ::::::::::::::::::::::::::::::::
         */

        // FEAT(chris): need the lists
        List<Path> Paths;
        Dictionary<string, Path> PathsByName;
        List<Skill> Skills;
        Dictionary<string, Skill> SkillsByName;
        List<Heritage> Heritages;
        Dictionary<string, Heritage> HeritagesByName;



        bool didSelect;

        /// <summary>
        /// Utility for accessing common client/server functionality.
        /// </summary>
        ICoreAPI api;

        Harmony harmony;

        public override void Start(ICoreAPI api)
        {
            // NOTE(Chris): The Start* methods of base are empty.
            this.api = api;

            // harmony = new Harmony(MOD_NAME);
            // harmony.PatchAll();

            api.Network
                .RegisterChannel("heritageselection")
                .RegisterMessageType<HeritageSelectionPacket>()
                .RegisterMessageType<HeritageSelectedState>();

        }

        // FIXME(chris): I think this can be removed eventually?
        // [HarmonyPostfix]
        // [HarmonyPatch(typeof(EntityPlayer), "EntityPlayer")]
        // public static void EntityPlayerInit(EntityPlayer __instance) {
        //     __instance.Stats
        //         .Register("combatant", EnumStatBlendType.FlatSum)
        //         .Register("farmer ", EnumStatBlendType.FlatSum)
        //         .Register("homekeeper", EnumStatBlendType.FlatSum)
        //         .Register("hunter", EnumStatBlendType.FlatSum)
        //         .Register("miner", EnumStatBlendType.FlatSum)
        //         .Register("processer", EnumStatBlendType.FlatSum)
        //         .Register("rancher", EnumStatBlendType.FlatSum)
        //         .Register("smith", EnumStatBlendType.FlatSum)
        //         .Register("woodsman", EnumStatBlendType.FlatSum);
        // }

        private void loadCharacterHeritages()
        {
            this.Paths = this.api.Assets
                .Get("rpskills:config/paths.json").ToObject<List<Path>>(null);
            api.Logger.Event("loaded paths");
            this.Skills = this.api.Assets
                .Get("rpskills:config/skills.json").ToObject<List<Skill>>(null);
            this.Heritages = this.api.Assets
                .Get("rpskills:config/heritages.json").ToObject<List<Heritage>>(null);

            PathsByName = new Dictionary<string, Path>();
            SkillsByName = new Dictionary<string, Skill>();
            HeritagesByName = new Dictionary<string, Heritage>();
            foreach (Skill skill in this.Skills)
            {
                this.SkillsByName[skill.Name] = skill;
            }

            foreach (Heritage heritage in this.Heritages)
            {
                this.HeritagesByName[heritage.Name] = heritage;
            }


            this.api.Logger.Debug("Heritages and Skills loaded!");
        }



        /*
        ::::::::::::::::::::::::::::::::
        :::::::::::::Client:::::::::::::
        ::::::::::::::::::::::::::::::::
         */


        ICoreClientAPI capi;

        GuiDialogCharacterBase charDlg;




        public override void StartClientSide(ICoreClientAPI api)
        {
            this.capi = api;
<<<<<<< Updated upstream
            _ = api.Network.GetChannel("heritageselection").SetMessageHandler<HeritageSelectedState>(new NetworkServerMessageHandler<HeritageSelectedState>(this.onSelectedState));
=======
            api.Network.GetChannel("heritageselection").SetMessageHandler<HeritageSelectedState>(new NetworkServerMessageHandler<HeritageSelectedState>(this.onSelectedState));
>>>>>>> Stashed changes

            // WARN(chris): uncommenting may suck
            // api.Event.IsPlayerReady += this.Event_IsPlayerReady;
            // api.Event.PlayerJoin += this.Event_PlayerJoin;

            // FEAT(chris): primary functionality of the branch
            api.Event.BlockTexturesLoaded += this.loadCharacterHeritages;
<<<<<<< Updated upstream
            this.charDlg = api.Gui.LoadedGuis.Find((GuiDialog dlg) => dlg is GuiDialogCharacterBase) as GuiDialogCharacterBase;
            this.charDlg.Tabs.RemoveAll((GuiTab tab) => tab.Name.Equals(Vintagestory.API.Config.Lang.Get("charactertab-traits", Array.Empty<object>())));
=======
            // this.charDlg = api.Gui.LoadedGuis.Find((GuiDialog dlg) => dlg is GuiDialogCharacterBase) as GuiDialogCharacterBase;
            // this.charDlg.Tabs.RemoveAll((GuiTab tab) => tab.Name.Equals(Vintagestory.API.Config.Lang.Get("charactertab-traits", Array.Empty<object>())));
>>>>>>> Stashed changes
        }


        private void onSelectedState(HeritageSelectedState s)
        {
            this.api.Logger.Debug("Recieved status of heriatge selection");
            this.didSelect = s.DidSelect;
        }



        /*
        ::::::::::::::::::::::::::::::::
        :::::::::::::Server:::::::::::::
        ::::::::::::::::::::::::::::::::
         */

        ICoreServerAPI sapi;




        public override void StartServerSide(ICoreServerAPI api)
        {
            this.sapi = api;

            // WARN(chris): uncommenting may suck
<<<<<<< Updated upstream
            _ = api.Network.GetChannel("heritageselection")
=======
            api.Network.GetChannel("heritageselection")
>>>>>>> Stashed changes
                .SetMessageHandler<HeritageSelectionPacket>(
                    new NetworkClientMessageHandler<HeritageSelectionPacket>(
                        this.onHeritageSelection
                    )
                );

            // WARN(chris): uncommenting may suck
            api.Event.PlayerJoin += this.Event_PlayerJoinServer;

            // FEAT(chris): primary functionality of the branch
            api.Event.ServerRunPhase(EnumServerRunPhase.ModsAndConfigReady, new Action(this.loadCharacterHeritages));
        }

        private void onHeritageSelection(IServerPlayer fromPlayer, HeritageSelectionPacket packet)
        {
            throw new NotImplementedException();
        }



        /*
        ::::::::::::::::::::::::::::::::
        :::::::::::::Events:::::::::::::
        ::::::::::::::::::::::::::::::::
         */




        // private bool Event_IsPlayerReady(ref EnumHandling handling)
        // {
        //     if (this.didSelect)
        //     {
        //         return true;
        //     }
        //     handling = EnumHandling.PreventDefault;
        //     return false;
        // }

        // private void Event_PlayerJoin(IClientPlayer byPlayer)
        // {
        //     if (!this.didSelect && byPlayer.PlayerUID == this.capi.World.Player.PlayerUID)
        //     {
        //         // TODO(chris): not my place...
        //         // CharacterSystem.Event_PlayerJoin for reference. Looks like
        //         // it sets up GUI stuff
        //     }
        //     throw new NotImplementedException();
        // }

        public void Event_PlayerJoinServer(IServerPlayer byPlayer)
        {
            throw new NotImplementedException("Hello!");
        }



        /*
        ::::::::::::::::::::::::::::::::
        ::::::::::::::Tidy::::::::::::::
        ::::::::::::::::::::::::::::::::
         */

        public override void Dispose()
        {
            base.Dispose();
            harmony.UnpatchAll(MOD_NAME);
        }

    }
}
