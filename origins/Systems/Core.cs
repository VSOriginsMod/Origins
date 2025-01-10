using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.Common;

//TODO(chris): refactor naming scheme: "Heratige" => "Origins"
//NOTE(chris): all current WARN(chris) in this file indicates client-server
//              interactions. They depend on the network channel feature, which
//              must be created and debugged first.

namespace Origins.Systems
{

    // TODO (chris): add any kinda network-based config (e.g. Vintagestory.GameContent.SurvivalConfig)
    // source: https://github.com/anegostudios/vssurvivalmod/blob/master/Systems/Core.cs

    /// <summary>
    /// ModSystem is the base for any VintageStory code mods.
    /// </summary>
    public class OriginsCoreSystem : ModSystem
    {
        public const string Domain = "origins";
        public const string ModName = "Origins";
        public const string ChannelOriginsCore = "origins-core";

        private static IEnumerable<Type> patchTypes = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(t => null != t.GetInterface("IPatch"));


        private ICoreAPI api;
        private ICoreServerAPI sapi;

        // // FEAT(chris): need the lists
        // private List<SkillPath> SkillPaths;
        // private Dictionary<string, SkillPath> SkillPathsByName;



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
            api.Assets.AddModOrigin(Domain, Path.Combine(Vintagestory.API.Config.GamePaths.AssetsPath, "origins"));
        }

        public override void Start(ICoreAPI api)
        {
            this.api = api;

            api.Network.RegisterChannel(ChannelOriginsCore);

        }

        public override void AssetsFinalize(ICoreAPI api)
        {
            // apply patches
            ExecutePatches(api);
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            // adds paths, skills, origins
            api.Event.BlockTexturesLoaded += LoadProgressionSystems;

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
            sapi = api;

            // adds paths, skills, origins
            api.Event.ServerRunPhase(
                EnumServerRunPhase.ModsAndConfigReady,
                new Action(LoadProgressionSystems)
            );

            Commands.LoadCommands(sapi);
        }

        private void LoadProgressionSystems()
        {
            OriginsLogger.Debug(api, "At this point, progression systems will load when implemented");
            /* IGNORE FOR SAKE OF CLARITY */
            // this.SkillPaths = this.api.Assets
            //     .Get("origins:config/paths.json").ToObject<List<SkillPath>>(null);
            // ModLogging.Debug(api, "Paths loaded");
            // SkillPathsByName = new Dictionary<string, SkillPath>();
            // foreach (SkillPath path in this.SkillPaths)
            // {
            //     this.SkillPathsByName[path.Name] = path;
            // }
            /* IGNORE FOR SAKE OF CLARITY */

            // SkillSystem.Build(api);
            // OriginSystem.Build(api);
        }

        public static void ExecutePatches(ICoreAPI api)
        {
            // only want to patch on server-side
            if (EnumAppSide.Server != api.Side)
            {
                return;
            }

            foreach (var patch in patchTypes)
            {
                try
                {
                    OriginsLogger.Debug(api, "Attempting to apply following code patch: " + patch.Name);
                    patch
                        .GetMethod("RegisterPatch", new[] { typeof(ICoreAPI) })
                        .Invoke(null, new object[] { api });
                }
                catch (Exception err)
                {
                    OriginsLogger.Debug(api, "Oops! " + patch.Name + " was not recognized by this process... Does it implement Origins.Patches.IPatch");
                    OriginsLogger.Error(api, err);
                }

            }
        }

        public static void ExecutePatches()
        {
            // only want to patch on server-side
            // if (EnumAppSide.Server != api.Side)
            // {
            //     return;
            // }

            foreach (var patch in patchTypes)
            {
                try
                {
                    // OriginsLogger.Debug(api, "Attempting to apply following code patch: " + patch.Name);
                    patch
                        .GetMethod("RegisterPatch", new Type[] { })
                        .Invoke(null, new Type[] { });
                }
                catch (Exception err)
                {
                    // OriginsLogger.Debug(api, "Oops! " + patch.Name + " was not recognized by this process... Does it implement Origins.Patches.IPatch");
                    // OriginsLogger.Error(api, err);
                }

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

    class OriginsLogger
    {
        private const string Identifier = "[" + OriginsCoreSystem.Domain + "] ";

        internal static void Debug(ICoreAPI api, string message)
        {
            api.Logger.Debug(Identifier + message);
        }

        internal static void Error(ICoreAPI api, Exception err)
        {
            api.Logger.Error(err);
        }
    }
}
