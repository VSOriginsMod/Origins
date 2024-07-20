using ProtoBuf;
using rpskills;
using rpskills.CoreSys;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace origins
{
    internal class OriginSystem : ProgressionSystem<Origin>
    {
        private const string CFG = "chooseOrigin";
        private const string ORIGIN_NETWORK_CHANNEL = CoreSystem.CHANNEL_CORE_RPSKILLS;
        private static INetworkChannel NetworkChannel;

        internal static void Build(ICoreAPI api)
        {
            ProgressionSystem<Origin>.Load(api, "origins:config/origins.json");

            api.Logger.Event("loaded origins");
        }

        public static Origin GetOrigin(string name)
        {
            return ElementsByName[name];
        }

        /// <summary>
        /// Runs during Start phase of CoreSystem (maybe rename to SysCore?)
        /// to inform both client and server of network protocols.
        /// </summary>
        /// <param name="api"></param>
        internal static void NetworkRegistration(ICoreAPI api)
        {
            // ensure network channel exists
            NetworkChannel = api.Network.GetChannel(ORIGIN_NETWORK_CHANNEL);
            NetworkChannel ??= api.Network.RegisterChannel(ORIGIN_NETWORK_CHANNEL);

            NetworkChannel
                .RegisterMessageType<OriginSelectionPacket>()
                .RegisterMessageType<OriginSelectionState>();
        }


        private bool OriginSelected = false;

        public OriginSystem(ICoreAPI api) : base(api)
        {
        }

        internal override void ClientInit(ICoreClientAPI capi)
        {
            capi.Network.GetChannel(ORIGIN_NETWORK_CHANNEL)
                    .SetMessageHandler<OriginSelectionState>(
                        new NetworkServerMessageHandler<OriginSelectionState>(
                            CHOriginSelected
                    ));

            capi.Event.IsPlayerReady += CEIsPlayerReady;
            capi.Event.PlayerJoin += CEPlayerJoin;

        }

        internal override void ServerInit(ICoreServerAPI sapi)
        {
            // NOTE(chris): this big block tells the server how to reply to
            //              incoming packets with respect to origin selection
            sapi.Network.GetChannel(ORIGIN_NETWORK_CHANNEL)
                .SetMessageHandler<OriginSelectionPacket>(
                    new NetworkClientMessageHandler<OriginSelectionPacket>(
                        SHOriginSelected
                ));

            // NOTE(chris): tells the server what to do when a player connects
            sapi.Event.PlayerJoin += SEPlayerJoin;
        }

        public void CHOriginSelected(OriginSelectionState server_state)
        {
            this.OriginSelected = server_state.HasSelected;
        }

        /// <summary>
        /// here, we want to make sure all of the player origin data is
        /// selected and valid. If handling is set to `PreventDefault`, then
        /// the system must eventually call `Network.SendPlayerNowReady()`!
        /// </summary>
        /// <param name="handling">server's understanding of client readiness</param>
        /// <returns></returns>
        private bool CEIsPlayerReady(ref EnumHandling handling)
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

        private void CEPlayerJoin(IClientPlayer byPlayer)
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
            // tell the server what the player selected
            OriginSelected = true;
            OriginSelectionPacket p = new OriginSelectionPacket
            {
                DidSelect = OriginSelected,
                OriginName = "average",
            };

            capi.Network.GetChannel(ORIGIN_NETWORK_CHANNEL)
                .SendPacket(p);
            capi.Network.SendPlayerNowReady();

            guiStuff_OnClose.Invoke();
        }

        /// <summary>
        /// how the server handles a player selecting a origin. this is the
        /// wrapper for character origin 'setter'
        /// </summary>
        /// <param name="fromPlayer">packet-emitting client</param>
        /// <param name="packet">origin selection data</param>
        /// <exception cref="NotImplementedException">You Should Not See This in dev</exception>
        private void SHOriginSelected(IServerPlayer fromPlayer, OriginSelectionPacket packet)
        {
            bool RemembersOriginSelection = SerializerUtil.Deserialize<bool>(
                fromPlayer.GetModdata(CFG), false
            );

            // FIXME(chris): forcing OriginSelectionPacket to be processed
            RemembersOriginSelection = false;

            if (RemembersOriginSelection)
            {
                api.Logger.Warning("you've already chosen an origin");
                return;
            }

            api.Logger.Debug(fromPlayer.PlayerName + " is originally a(n) " + packet.OriginName);
            /*aosjdfklasdf;adskljfk;ladsjfk;ads;fkjads;kfjk;adlsjfk;ldasj;fja;kfkj;asjf;asldjf;jHELP*/
            if (packet.DidSelect)
            {
                fromPlayer.SetModdata(
                    CFG,
                    SerializerUtil.Serialize<bool>(packet.DidSelect)
                );

                //NOTE(chris): the following list is pulled from
                //CharacterSystem.onCharacterSelection. Use this list to
                //impl "Origin"s and "Skills", etc.

                //TODO(chris): use player.WatchedAttributes.SetString to store
                //              the origin name (setCharacterClass)

                api.Logger.Debug("Beginning to add skills");
                if (!SkillSystem.Loaded)
                {
                    api.Logger.Debug("building skill sys");

                    SkillSystem.Build(api);
                }

                foreach (Skill skill in SkillSystem.Elements)
                {
                    fromPlayer.WorldData.EntityPlayer.WatchedAttributes.RemoveAttribute("s_" + skill.Name);
                    api.Logger.Debug("Setting " + skill.Name + "@" + skill.Level);
                    fromPlayer.WorldData.EntityPlayer.WatchedAttributes.SetFloat("s_" + skill.Name, skill.Level);
                }

                //TODO(chris): next, attributes are to be applied
                //              (applyTraitAttributes)

                //TODO(chris): change entity behavior using
                //              fromPlayer.Entity.GetBehavior<T>()


            }
            //TODO(chris): mark all changed WatchedAttributes as dirty

            fromPlayer.BroadcastPlayerData(true);
        }

        /// <summary>
        /// sends the state of the character with respect to rpskills to the
        /// client, over Network:RPSKILLS_CORE_CHANNEL.
        /// </summary>
        /// <param name="byPlayer">the joining player</param>
        public void SEPlayerJoin(IServerPlayer byPlayer)
        {
            // WARN(chris): we are using Moddata(createCharacter) as a
            //              placeholder for now -- functionality is tied to
            //              VintageStory.GameContent.CharacterSystem
            OriginSelected = SerializerUtil.Deserialize<bool>(
                byPlayer.GetModdata("createCharacter"), false
            );

            // FIXME(chris): forcing origin selection every time client joins
            OriginSelected = false;

            if (!OriginSelected)
            {
                api.Logger.Debug("Character creation has not happened yet.");
            }
            else
            {
                api.Logger.Debug("Character creation has happened!");
            }

            (api as ICoreServerAPI).Network.GetChannel(ORIGIN_NETWORK_CHANNEL)
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

    public class Origin : INamedProgression
    {
        public string Name;

        /// <summary>
        /// key: skill; value: level
        /// </summary>
        public Dictionary<string, int> Skillset;

        string INamedProgression.Name => Name;
    }

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
}
