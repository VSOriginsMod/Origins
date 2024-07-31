using Origins.Config;
using System;
using System.Collections.Generic;
using System.IO;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

//TODO(chris): refactor naming scheme: "Heratige" => "Origins"
//NOTE(chris): all current WARN(chris) in this file indicates client-server
//              interactions. They depend on the network channel feature, which
//              must be created and debugged first.

namespace Origins
{

    // TODO (chris): add any kinda network-based config (e.g. Vintagestory.GameContent.SurvivalConfig)
    // source: https://github.com/anegostudios/vssurvivalmod/blob/master/Systems/Core.cs

    /// <summary>
    /// ModSystem is the base for any VintageStory code mods.
    /// </summary>
    public class OriginsCoreSystem : ModSystem
    {


        private ICoreAPI api;
        private ICoreServerAPI sapi;

        // FEAT(chris): need the lists
        private List<SkillPath> SkillPaths;
        private Dictionary<string, SkillPath> SkillPathsByName;



        public override double ExecuteOrder()
        {
            return base.ExecuteOrder();
        }

        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return true;
        }

        public override void StartPre(ICoreAPI api)
        {
            // When loaded, load origins' assets
            api.Assets.AddModOrigin(ModConstants.Domain, Path.Combine(Vintagestory.API.Config.GamePaths.AssetsPath, "origins"));
        }

        public override void Start(ICoreAPI api)
        {
            // NOTE(Chris): The Start* methods of base are empty.
            this.api = api;

            api.Network.RegisterChannel(ModConstants.ChannelOriginsCore);

        }

        public override void StartClientSide(ICoreClientAPI api)
        {
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

            // adds paths, skills, origins
            api.Event.ServerRunPhase(
                EnumServerRunPhase.ModsAndConfigReady,
                new Action(this.LoadProgressionSystems)
            );

            Commands.LoadCommands(sapi);
        }

        private void LoadProgressionSystems()
        {

            /* IGNORE FOR SAKE OF CLARITY */
            this.SkillPaths = this.api.Assets
                .Get("origins:config/paths.json").ToObject<List<SkillPath>>(null);
            ModLogging.Debug(api, "Paths loaded");
            SkillPathsByName = new Dictionary<string, SkillPath>();
            foreach (SkillPath path in this.SkillPaths)
            {
                this.SkillPathsByName[path.Name] = path;
            }
            /* IGNORE FOR SAKE OF CLARITY */

            // SkillSystem.Build(api);
            // OriginSystem.Build(api);
        }


        public static void ListWatchedAttributes(ICoreServerAPI api, IServerPlayer player)
        {
            foreach (var foo in player.Entity.WatchedAttributes.Keys)
            {
                api.Logger.Debug(foo);
            }
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

    public class SkillPath
    {
        public string Name;
        public string Value;
    }
}
