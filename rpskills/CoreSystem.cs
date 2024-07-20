using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using rpskills.CoreSys;
using origins;
using System.Collections.ObjectModel;
using System.Linq;

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
        public const string CHANNEL_CORE_RPSKILLS = "origins-core";


        /*
        ::::::::::::::::::::::::::::::::
        :::::::::::::Shared:::::::::::::
        ::::::::::::::::::::::::::::::::
         */

        private ICoreAPI api;
        private ICoreClientAPI capi;
        private ICoreServerAPI sapi;

        // TODO(chris): convert this to collection
        private OriginSystem originSystem;
        private SkillSystem skillSystem;

        // FEAT(chris): need the lists
        private List<Path> Paths;
        private Dictionary<string, Path> PathsByName;

        /// <summary>
        /// Utility for accessing common client/server functionality.
        /// </summary>
        public override void Start(ICoreAPI api)
        {
            // NOTE(Chris): The Start* methods of base are empty.
            this.api = api;

            api.Network.RegisterChannel(CHANNEL_CORE_RPSKILLS);

            OriginSystem.NetworkRegistration(api);

        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            this.capi = api;

            originSystem = new OriginSystem(capi);
            skillSystem = new SkillSystem(capi);

            // adds paths, skills, origins
            api.Event.BlockTexturesLoaded += this.LoadProgressionSystems;

            // NOTE(chris): the SendPlayerNowReady call is in the
            //              GuiDialogCharacterBase.OnGuiClose override for the
            //              CharacterSystem. The GuiDialog child is located
            //              at this point in the StartClientSide. See below:
            // this.charDlg = api.Gui.LoadedGuis
            //     .Find((GuiDialog dlg) => dlg is GuiDialogCharacterBase)
            //     as GuiDialogCharacterBase;
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            this.sapi = api;

            originSystem = new OriginSystem(sapi);

            // adds paths, skills, origins
            api.Event.ServerRunPhase(
                EnumServerRunPhase.ModsAndConfigReady,
                new Action(this.LoadProgressionSystems)
            );
        }

        private void LoadProgressionSystems()
        {

            /* IGNORE FOR SAKE OF CLARITY */
            this.Paths = this.api.Assets
                .Get("origins:config/paths.json").ToObject<List<Path>>(null);
            api.Logger.Event("loaded paths");
            PathsByName = new Dictionary<string, Path>();
            foreach (Path path in this.Paths)
            {
                this.PathsByName[path.Name] = path;
            }
            /* IGNORE FOR SAKE OF CLARITY */

            SkillSystem.Build(api);
            OriginSystem.Build(api);

            this.api.Logger.Debug("Origins and Skills loaded!");
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
}
