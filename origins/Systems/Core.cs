using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

//TODO(chris): refactor naming scheme: "Heratige" => "Origins"
//NOTE(chris): all current WARN(chris) in this file indicates client-server
//              interactions. They depend on the network channel feature, which
//              must be created and debugged first.

namespace origins
{
    /// <summary>
    /// ModSystem is the base for any VintageStory code mods.
    /// </summary>
    public class OriginsCoreSystem : ModSystem
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

            LoadCommands();
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
        ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        ::::::::::::::Just Don't Look Past This Line::::::::::::::
        ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
         */




        /// <summary>
        /// Must be called server-side!!!
        /// </summary>
        private void LoadCommands()
        {
            // create client commands
            IChatCommand get = api.ChatCommands.Create("get");
            get.RequiresPlayer();
            get.RequiresPrivilege(Privilege.root);
            get.WithDescription("Read WatchedAttributes of the caller.");
            get.HandleWith(args => {
                string cmdargs = args.RawArgs.PopAll();
                string result = "given " + cmdargs + "\n";
                EntityPlayer eplr = args.Caller.Player.WorldData.EntityPlayer;

                foreach (var attr in eplr.WatchedAttributes)
                {
                    result += attr.Key;
                    result += "\n";
                }

                return TextCommandResult.Success(result);
            });

            IChatCommand get_skill = get.BeginSubCommand("skill");
            get_skill.RequiresPrivilege(Privilege.root);
            get_skill.WithDescription("Read Origin Skills of the caller.");
            get_skill.HandleWith(args => {
                string result = "";
                EntityPlayer eplr = args.Caller.Player.WorldData.EntityPlayer;

                foreach (var attr in eplr.WatchedAttributes)
                {
                    if (!attr.Key.StartsWith("s_"))
                    {
                        continue;
                    }

                    result += attr.Key + ": " + attr.Value.ToString() + "\n";

                }

                return TextCommandResult.Success(result);
            });
            get_skill.EndSubCommand();


            get.Validate(); // name, priv, desc, handler




            IChatCommand set = api.ChatCommands.Create("set");
            set.RequiresPlayer();
            // set.WithArgs( populate with Skills )
            set.RequiresPrivilege("root");
            set.WithDescription("Resets Origin Skills of the caller.");
            set.HandleWith(args => {
                float new_val = 0f;
                string result = "";
                EntityPlayer eplr = args.Caller.Player.WorldData.EntityPlayer;

                foreach (Skill skill in SkillSystem.Elements)
                {
                    eplr.WatchedAttributes.SetFloat("s_" + skill.Name, new_val);
                }

                eplr.WatchedAttributes.MarkAllDirty();

                return TextCommandResult.Success(result);
            });

            IChatCommand set_skill = set.BeginSubCommand("skill");
            set_skill.RequiresPrivilege(Privilege.root);
            set_skill.WithDescription("Sets the given skill of the caller to a given value.");
            set_skill.HandleWith(args => {
                float new_val = 4f;
                string skill = "s_farmer";
                string result = "";
                EntityPlayer eplr = args.Caller.Player.WorldData.EntityPlayer;

                result += "set " + skill + " to lv " + new_val;
                eplr.WatchedAttributes.SetFloat(skill, new_val);

                return TextCommandResult.Success(result);
            });
            set_skill.EndSubCommand();


            set.Validate(); // name, priv, desc, handler



            IChatCommand del = api.ChatCommands.Create("del");
            del.RequiresPrivilege(Privilege.root);
            del.WithDescription("Deletes all Origin Skills from the caller's player data.");
            del.HandleWith(args => {
                string result = "";
                EntityPlayer eplr = args.Caller.Player.WorldData.EntityPlayer;

                foreach (Skill skill in SkillSystem.Elements)
                {
                    eplr.WatchedAttributes.RemoveAttribute("s_" + skill.Name);
                }

                return TextCommandResult.Success(result);
            });


            del.Validate(); // name, priv, desc, handler

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

    public class Path
    {
        public string Name;
        public string Value;
    }
}
