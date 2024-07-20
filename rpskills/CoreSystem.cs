using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using rpskills.CoreSys;

//TODO(chris): refactor naming scheme: "Heratige" => "Origins"
//NOTE(chris): all current WARN(chris) in this file indicates client-server
//              interactions. They depend on the network channel feature, which
//              must be created and debugged first.

namespace rpskills
{
    /// <summary>
    /// ModSystem is the base for any VintageStory code mods.
    /// </summary>
    public class CoreSystem : ModSystem
    {

        /*
        ::::::::::::::::::::::::::::::::
        ::::::::::::Constant::::::::::::
        ::::::::::::::::::::::::::::::::
         */

        private const string MOD_NAME = "Origins";
        private const string CHANNEL_CORE_RPSKILLS = "origins-core";
        private const string CFG_ORIGIN = "chooseOrigin";


        /*
        ::::::::::::::::::::::::::::::::
        :::::::::::::Shared:::::::::::::
        ::::::::::::::::::::::::::::::::
         */

        private ICoreAPI api;
        private ICoreClientAPI capi;
        private ICoreServerAPI sapi;

        private PlayerSkillsUI playerSkillsUI;

        // FEAT(chris): need the lists
        private List<Path> Paths;
        private Dictionary<string, Path> PathsByName;
        private List<Skill> Skills;
        private Dictionary<string, Skill> SkillsByName;
        private List<Origin> Origins;
        private Dictionary<string, Origin> OriginsByName;



        private bool didSelect;

        /// <summary>
        /// Utility for accessing common client/server functionality.
        /// </summary>

        public override void Start(ICoreAPI api)
        {
            // NOTE(Chris): The Start* methods of base are empty.
            this.api = api;

            api.Network
                .RegisterChannel(CHANNEL_CORE_RPSKILLS)
                .RegisterMessageType<OriginSelectionPacket>()
                .RegisterMessageType<OriginSelectedState>();

        }

        private void LoadCharacterOrigins()
        {
            this.Paths = this.api.Assets
                .Get("rpskills:config/paths.json").ToObject<List<Path>>(null);
            api.Logger.Event("loaded paths");
            PathsByName = new Dictionary<string, Path>();
            foreach (Path path in this.Paths)
            {
                this.PathsByName[path.Name] = path;
            }

            this.Skills = this.api.Assets
                .Get("rpskills:config/skills.json").ToObject<List<Skill>>(null);
            SkillsByName = new Dictionary<string, Skill>();
            foreach (Skill skill in this.Skills)
            {
                this.SkillsByName[skill.Name] = skill;
            }
            api.Logger.Event("loaded skills");

            this.Origins = this.api.Assets
                .Get("rpskills:config/origins.json").ToObject<List<Origin>>(null);
            OriginsByName = new Dictionary<string, Origin>();
            foreach (Origin origin in this.Origins)
            {
                this.OriginsByName[origin.Name] = origin;
            }
            api.Logger.Event("loaded origins");


            this.api.Logger.Debug("Origins and Skills loaded!");
        }



        /*
        ::::::::::::::::::::::::::::::::
        :::::::::::::Client:::::::::::::
        ::::::::::::::::::::::::::::::::
         */

        public override void StartClientSide(ICoreClientAPI api)
        {
            this.capi = api;

            // tell client how to handle server sending origin information
            api.Network
                .GetChannel(CHANNEL_CORE_RPSKILLS)
                .SetMessageHandler<OriginSelectedState>(
                    new NetworkServerMessageHandler<OriginSelectedState>(
                        this.OnSelectedState
                ));

            api.Event.IsPlayerReady += this.Event_IsPlayerReady;
            api.Event.PlayerJoin += this.Event_PlayerJoin;

            // FEAT(chris): primary functionality of the branch
            api.Event.BlockTexturesLoaded += this.LoadCharacterOrigins;

            //Note(Moon):
            //these lines are what's needed in order to turn the dialog box, the initialization
            //of the PlayerSkillsUI can be moved to a seprate class and likely will be at a later date.
            //It just needs the capi in order to be be hooked for the hotkey.
            playerSkillsUI = new PlayerSkillsUI(capi);
            capi.Input.RegisterHotKey("Skill Interface", "Opens up the Skills GUI", GlKeys.O, HotkeyType.GUIOrOtherControls);
            capi.Input.SetHotKeyHandler("Skill Interface", ToggleGUI);

            // NOTE(chris): the SendPlayerNowReady call is in the
            //              GuiDialogCharacterBase.OnGuiClose override for the
            //              CharacterSystem. The GuiDialog child is located
            //              at this point in the StartClientSide. See below:
            // this.charDlg = api.Gui.LoadedGuis
            //     .Find((GuiDialog dlg) => dlg is GuiDialogCharacterBase)
            //     as GuiDialogCharacterBase;
        }

        public bool ToggleGUI(KeyCombination comb)
        {
            if (playerSkillsUI.IsOpened())
                playerSkillsUI.TryClose();
            else
                playerSkillsUI.TryOpen();

            return true;
        }

        private void OnSelectedState(OriginSelectedState s)
        {
            this.api.Logger.Debug("Recieved status of origin selection: " + s.DidSelect);
            this.didSelect = s.DidSelect;
        }



        /*
        ::::::::::::::::::::::::::::::::
        :::::::::::::Server:::::::::::::
        ::::::::::::::::::::::::::::::::
         */

        public override void StartServerSide(ICoreServerAPI api)
        {
            this.sapi = api;

            // NOTE(chris): this big block tells the server how to reply to
            //              incoming packets with respect to origin selection
            api.Network.GetChannel(CHANNEL_CORE_RPSKILLS)
                .SetMessageHandler<OriginSelectionPacket>(
                    new NetworkClientMessageHandler<OriginSelectionPacket>(
                        this.OnOriginSelection
                )
            );

            // NOTE(chris): tells the server what to do when a player connects
            api.Event.PlayerJoin += this.Event_PlayerJoinServer;

            // FEAT(chris): primary functionality of the branch
            api.Event.ServerRunPhase(
                EnumServerRunPhase.ModsAndConfigReady,
                new Action(this.LoadCharacterOrigins)
            );
        }

        /// <summary>
        /// how the server handles a player selecting a origin. this is the
        /// wrapper for character origin 'setter'
        /// </summary>
        /// <param name="fromPlayer">packet-emitting client</param>
        /// <param name="packet">origin selection data</param>
        /// <exception cref="NotImplementedException">You Should Not See This in dev</exception>
        private void OnOriginSelection(IServerPlayer fromPlayer, OriginSelectionPacket packet)
        {
            bool didSelectBefore = SerializerUtil.Deserialize<bool>(
                fromPlayer.GetModdata(CFG_ORIGIN), false
            );

            if (didSelectBefore) {
                api.Logger.Warning("you've already chosen an origin");
                return;
            }

            api.Logger.Debug("successfully chosen " + packet.OriginName);

            if(packet.DidSelect) {
                fromPlayer.SetModdata(
                    CFG_ORIGIN,
                    SerializerUtil.Serialize<bool>(packet.DidSelect)
                );

                //NOTE(chris): the following list is pulled from
                //CharacterSystem.onCharacterSelection. Use this list to
                //impl "Origin"s and "Skills", etc.

                //TODO(chris): use player.WatchedAttributes.SetString to store
                //              the origin name (setCharacterClass)

                //TODO(chris): next, attributes are to be applied
                //              (applyTraitAttributes)

                //TODO(chris): change entity behavior using
                //              fromPlayer.Entity.GetBehavior<T>()


            }
            //TODO(chris): mark all changed WatchedAttributes as dirty

            fromPlayer.BroadcastPlayerData(true);
        }



        /*
        ::::::::::::::::::::::::::::::::
        :::::::::::::Events:::::::::::::
        ::::::::::::::::::::::::::::::::
         */



        /// <summary>
        /// here, we want to make sure all of the player origin data is
        /// selected and valid. If handling is set to `PreventDefault`, then
        /// the system must eventually call `Network.SendPlayerNowReady()`!
        /// </summary>
        /// <param name="handling">server's understanding of client readiness</param>
        /// <returns></returns>
        private bool Event_IsPlayerReady(ref EnumHandling handling)
        {
            if (this.didSelect)
            {
                return true;
            }
            // WARN(chris): IClientNetworkAPI will now expects a
            //              `SendPlayerNowReady` call before it will let a
            //              player join a world!!!
            handling = EnumHandling.PreventDefault;
            return false;
        }

        private void Event_PlayerJoin(IClientPlayer byPlayer)
        {
            if (this.didSelect && byPlayer.PlayerUID == this.capi.World.Player.PlayerUID)
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
                this.capi.PauseGame(false);
            };

            this.capi.Event.EnqueueMainThreadTask(delegate
            {
                this.capi.PauseGame(true);
            }, "pausegame");

            // TODO(chris): please hide the following in the Gui code. The
            //              remaining code in this function shouldn't be here!!!

            // NOTE(chris): these next two lines should stay directly next to
            //              eachother in the Gui code, immediately after client
            //              selection is done with Origin selection.
            // WARN(chris): these are default values, use the Gui to get
            //              player-chosen values to put here.
            // tell the server what the player selected
            didSelect = true;
            OriginSelectionPacket p = new OriginSelectionPacket {
                DidSelect = didSelect,
                OriginName = this.OriginsByName["average"].Name,
            };
            capi.Network
                .GetChannel(CHANNEL_CORE_RPSKILLS)
                .SendPacket<OriginSelectionPacket>
                (
                    p
                );
            capi.Network.SendPlayerNowReady();

            guiStuff_OnClose.Invoke();
        }

        /// <summary>
        /// sends the state of the character with respect to rpskills to the
        /// client, over Network:RPSKILLS_CORE_CHANNEL.
        /// </summary>
        /// <param name="byPlayer">the joining player</param>
        public void Event_PlayerJoinServer(IServerPlayer byPlayer)
        {
            // WARN(chris): we are using Moddata(createCharacter) as a
            //              placeholder for now -- functionality is tied to
            //              VintageStory.GameContent.CharacterSystem
            this.didSelect = SerializerUtil.Deserialize<bool>(
                byPlayer.GetModdata("createCharacter"), false
            );
            if (!this.didSelect) {
                api.Logger.Debug("Character creation has not happened yet.");
            } else {
                api.Logger.Debug("Character creation has happened!");
            }

            this.sapi.Network
                .GetChannel(CHANNEL_CORE_RPSKILLS)
                .SendPacket<OriginSelectedState>(
                    new OriginSelectedState
                    {
                        DidSelect = this.didSelect
                    },
                    new IServerPlayer[]
                    {
                        byPlayer
                    }
                );
            api.Logger.Debug("Package sent indicating selection status");
        }


        /*
        ::::::::::::::::::::::::::::::::
        ::::::::::::::Tidy::::::::::::::
        ::::::::::::::::::::::::::::::::
         */

        public override void Dispose()
        {
            base.Dispose();
        }

    }

    namespace Dummy
    {

    }
}
