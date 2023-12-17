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

// TODO(chris): delete all FEAT(chris) annotations before merging
// NOTE(chris): all current WARN(chris) in this file indicates client-server
//              interactions. They depend on the network channel feature, which
//              must be created and debugged first.

namespace rpskills
{
    /// <summary>
    /// ModSystem is the base for any VintageStory code mods.
    /// </summary>
    // /// HarmonyPatch is required for any class that patches
    // [HarmonyPatch]
    public class HeritageSystem : ModSystem
    {

        const string MOD_NAME = "rpskills";


        // TODO(Chris): "HarmonyPostix" on
        // VintageStory.GameContent.CharacterSystem.loadCharacterClasses() to
        // load "Heritages" and "skills" into our parallel system

        // NOTE(Chris): Our Heritages and skills seem to be analogous to VS's
        // "classes" and "traits"

        // NOTE(Chris): can all of the ICoreAPI variables be unionized in a record?
        // record API {
        //     record ServerAPI() : API();
        //     record CommonAPI() : API();
        //     record ClientAPI() : API();

        //     private API() {}
        // }







        /*
        ::::::::::::::::::::::::::::::::
        :::::::::::::Shared:::::::::::::
        ::::::::::::::::::::::::::::::::
         */
        
        // TODO(chris): move to its own file
        public class Heritage
        {
            public string Name;

            /// <summary>
            /// key: skill; value: level
            /// </summary>
            public Dictionary<string, int> Skillset;
        }

        // TODO(chris): move to its own file
        public class Skill
        {
            public string Name;

            public int Level;

            /// <summary>
            /// key: attribute; value: modifier
            /// </summary>
            public Dictionary<string, int> Attributes;
        }

        // FEAT(chris): need the lists
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

            harmony = new Harmony(MOD_NAME);

            api.Network.RegisterChannel("heritageselection")
                .RegisterMessageType<HeritageSelectionPacket>()
                .RegisterMessageType<HeritageSelectedState>();

        }

        // FIXME(chris): I think this can be removed eventually?
        [HarmonyPostfix]
        [HarmonyPatch(typeof(EntityPlayer), "EntityPlayer")]
        public static void EntityPlayerInit(EntityPlayer __instance) {
            __instance.Stats
                .Register("combatant", EnumStatBlendType.FlatSum)
                .Register("farmer ", EnumStatBlendType.FlatSum)
                .Register("homekeeper", EnumStatBlendType.FlatSum)
                .Register("hunter", EnumStatBlendType.FlatSum)
                .Register("miner", EnumStatBlendType.FlatSum)
                .Register("processer", EnumStatBlendType.FlatSum)
                .Register("rancher", EnumStatBlendType.FlatSum)
                .Register("smith", EnumStatBlendType.FlatSum)
                .Register("woodsman", EnumStatBlendType.FlatSum);
        }

        private void loadCharacterHeritages()
        {
            this.Skills = this.api.Assets.Get("config/skills.json").ToObject<List<Skill>>(null);
            this.Heritages = this.api.Assets.Get("config/heritages.json").ToObject<List<Heritage>>(null);

            foreach (Skill skill in this.Skills)
            {
                this.SkillsByName[skill.Name] = skill;
            }

            foreach (Heritage heritage in this.Heritages)
            {
                this.HeritagesByName[heritage.Name] = heritage;
            }
        }



        /*
        ::::::::::::::::::::::::::::::::
        :::::::::::::Client:::::::::::::
        ::::::::::::::::::::::::::::::::
         */

         
        ICoreClientAPI capi;




        public override void StartClientSide(ICoreClientAPI api)
        {
            this.capi = api;

            // WARN(chris): uncommenting may suck
            // api.Event.IsPlayerReady += this.Event_IsPlayerReady;
            // api.Event.PlayerJoin += this.Event_PlayerJoin;

            // FEAT(chris): primary functionality of the branch
            api.Event.BlockTexturesLoaded += this.loadCharacterHeritages;
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
            // api.Network.GetChannel("heritageselection")
            //     .SetMessageHandler<HeritageSelectionPacket>(
            //         new NetworkClientMessageHandler<HeritageSelectionPacket>(
            //             this.onHeritageSelection
            //         )
            //     );
            
            // WARN(chris): uncommenting may suck
            // api.Event.PlayerJoin += this.Event_PlayerJoinServer;

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




        private bool Event_IsPlayerReady(ref EnumHandling handling)
        {
            if (this.didSelect)
            {
                return true;
            }
            handling = EnumHandling.PreventDefault;
            return false;
        }

        private void Event_PlayerJoin(IClientPlayer byPlayer)
        {
            if (!this.didSelect && byPlayer.PlayerUID == this.capi.World.Player.PlayerUID)
            {
                // TODO(chris): not my place...
                // CharacterSystem.Event_PlayerJoin for reference. Looks like
                // it sets up GUI stuff
            }
            throw new NotImplementedException();
        }

        public void Event_PlayerJoinServer(IServerPlayer byPlayer)
        {
            throw new NotImplementedException();
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
