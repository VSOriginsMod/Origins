using Origins.Config;
using ProtoBuf;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace Origins.Character
{
    class OriginSelectionState
    {
        public bool HasSelected;
    }

    /// <summary>
    /// Contains all data regarding Origin selection.
    /// 
    /// See Vintagestory.GameContent.CharacterSelectionPacket for more details.
    /// </summary>
    // TODO(chris): What is this? I found it on CharacterSelectionPacket, and
    //              a constructor was "implicitly defined." For now, I'll
    //              explicitly define.
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class OriginSelectionPacket
    {
        public bool DidSelect;
        public string OriginName;
    }


    public class Origin // : IAttribute
    {
        public string Name;

        /// <summary>
        /// key: skill; value: level
        /// </summary>
        public Dictionary<string, int> Skillset;

    }


    internal class OriginSystem : ModSystem
    {
        private ICoreAPI api;

        private bool OriginSelected = false;

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
            this.api = api;

            api.Network.GetChannel(ModConstants.ChannelOriginsCore)
                .RegisterMessageType<OriginSelectionPacket>()
                .RegisterMessageType<OriginSelectionState>();
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            api.Network.GetChannel(ModConstants.ChannelOriginsCore)
                    .SetMessageHandler<OriginSelectionState>(
                        new NetworkServerMessageHandler<OriginSelectionState>(
                            CHandle_OriginSelected
                    ));

            api.Event.IsPlayerReady += CEvent_IsPlayerReady;
            api.Event.PlayerJoin += CEvent_PlayerJoin;
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            // NOTE(chris): this big block tells the server how to reply to
            //              incoming packets with respect to origin selection
            api.Network.GetChannel(ModConstants.ChannelOriginsCore)
                .SetMessageHandler<OriginSelectionPacket>(
                    new NetworkClientMessageHandler<OriginSelectionPacket>(
                        SHandle_OriginSelected
                ));

            // NOTE(chris): tells the server what to do when a player connects
            api.Event.PlayerJoin += SEvent_PlayerJoin;
        }

        public override void AssetsLoaded(ICoreAPI api)
        {
            ModLogging.Debug(api, "Origins loaded");
        }


        public void CHandle_OriginSelected(OriginSelectionState server_state)
        {
            this.OriginSelected = server_state.HasSelected;
        }

        /// <summary>
        /// how the server handles a player selecting a origin. this is the
        /// wrapper for character origin 'setter'
        /// </summary>
        /// <param name="fromPlayer">packet-emitting client</param>
        /// <param name="packet">origin selection data</param>
        /// <exception cref="NotImplementedException">You Should Not See This in dev</exception>
        private void SHandle_OriginSelected(IServerPlayer fromPlayer, OriginSelectionPacket packet)
        {
            bool RemembersOriginSelection = SerializerUtil.Deserialize<bool>(
                fromPlayer.GetModdata("OriginSelected"), false
            );

            // FIXME(chris): forcing OriginSelectionPacket to be processed
            // RemembersOriginSelection = true;

            if (RemembersOriginSelection)
            {
                api.Logger.Debug("you've already chosen an origin");
                return;
            }

            api.Logger.Debug(fromPlayer.PlayerName + " is originally a(n) " + packet.OriginName);

            if (packet.DidSelect)
            {
                fromPlayer.SetModdata(
                    "OriginSelected",
                    SerializerUtil.Serialize<bool>(packet.DidSelect)
                );

                //NOTE(chris): the following list is pulled from
                //CharacterSystem.onCharacterSelection. Use this list to
                //impl "Origin"s and "Skills", etc.

                fromPlayer.WorldData.EntityPlayer.WatchedAttributes.SetString("Origin", packet.OriginName);
                SkillSystem instance = (SkillSystem) api.ModLoader.GetModSystem("Origins.Character.SkillSystem");
                instance.InitializePlayer(fromPlayer);

                api.Logger.Debug("Initializing player skill data for " + fromPlayer.PlayerName);

                //TODO(chris): next, attributes are to be applied
                //              (applyTraitAttributes)

                //TODO(chris): change entity behavior using
                //              fromPlayer.Entity.GetBehavior<T>()


            }
            //TODO(chris): mark all changed WatchedAttributes as dirty

            fromPlayer.BroadcastPlayerData(true);
        }

        /// <summary>
        /// here, we want to make sure all of the player origin data is
        /// selected and valid. If handling is set to `PreventDefault`, then
        /// the system must eventually call `Network.SendPlayerNowReady()`!
        /// </summary>
        /// <param name="handling">server's understanding of client readiness</param>
        /// <returns></returns>
        private bool CEvent_IsPlayerReady(ref EnumHandling handling)
        {
            if (OriginSelected)
            {
                return true;
            }
            // WARN(chris): IClientNetworkAPI will now expects a
            //              `SendPlayerNowReady` call before it will let a
            //              player join a world!!!
            handling = EnumHandling.PreventDefault;
            return false;
        }

        private void CEvent_PlayerJoin(IClientPlayer byPlayer)
        {
            ICoreClientAPI capi = api as ICoreClientAPI;
            if (OriginSelected && byPlayer.PlayerUID == capi.World.Player.PlayerUID)
            {
                return;
            }
            //TODO(chris): not my place...
            //CharacterSystem.Event_PlayerJoin for reference. Looks like
            //it sets up GUI stuff. The game is paused when the Guis are
            //made, and Action GuiDialogue.OnClose gets an anonomys
            //delegate to unpause the game. Below is the boilerplate:

            Action guiStuff_OnClose = delegate
            {
                capi.PauseGame(false);
            };

            capi.Event.EnqueueMainThreadTask(delegate
            {
                capi.PauseGame(true);
            }, "pausegame");

            // TODO(chris): please hide the following in the Gui code. The
            //              remaining code in this function shouldn't be here!!!

            // NOTE(chris): these next two lines should stay directly next to
            //              eachother in the Gui code, immediately after client
            //              selection is done with Origin selection.
            // WARN(chris): these are default values, use the Gui to get
            //              player-chosen values to put here.

            // at some point we tell the server what the player selected
            OriginSelected = true;
            OriginSelectionPacket p = new OriginSelectionPacket
            {
                DidSelect = OriginSelected,
                OriginName = "average",
            };

            capi.Network.GetChannel(ModConstants.ChannelOriginsCore)
                .SendPacket(p);

            
            capi.Network.SendPlayerNowReady();

            guiStuff_OnClose.Invoke();
        }

        /// <summary>
        /// sends the state of the character with respect to rpskills to the
        /// client, over Network:RPSKILLS_CORE_CHANNEL.
        /// </summary>
        /// <param name="byPlayer">the joining player</param>
        public void SEvent_PlayerJoin(IServerPlayer byPlayer)
        {
            // WARN(chris): we are using Moddata(createCharacter) as a
            //              placeholder for now -- functionality is tied to
            //              VintageStory.GameContent.CharacterSystem
            OriginSelected = SerializerUtil.Deserialize<bool>(
                byPlayer.GetModdata("OriginSelected"), false
            );

            if (!OriginSelected)
            {
                api.Logger.Debug("Character creation has not happened yet.");
            }
            else
            {
                api.Logger.Debug("Character creation has happened already.");
            }

            // tell byPlayer's client their game state
            (api as ICoreServerAPI).Network.GetChannel(ModConstants.ChannelOriginsCore)
                .SendPacket(
                    new OriginSelectionState
                    {
                        HasSelected = OriginSelected
                    },
                    new IServerPlayer[]
                    {
                        byPlayer
                    }
                );
            api.Logger.Debug("Package sent indicating selection status");
        }
    }
}
