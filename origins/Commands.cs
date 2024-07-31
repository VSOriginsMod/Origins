using Origins.Character;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace Origins
{
    internal class Commands
    {
        /// <summary>
        /// Must be called server-side!!!
        /// </summary>
        public static void LoadCommands(ICoreServerAPI api)
        {
            // create client commands
            CreateGetCommand(api);
            CreateSetCommand(api);
            CreateDelCommand(api);
        }

        private static void CreateGetCommand(ICoreServerAPI api)
        {
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
        }

        private static void CreateSetCommand(ICoreServerAPI api)
        {
            IChatCommand set = api.ChatCommands.Create("set");
            set.RequiresPlayer();
            // set.WithArgs( populate with Skills )
            set.RequiresPrivilege("root");
            set.WithDescription("Resets Origin Skills of the caller.");
            set.HandleWith(args => {
                float new_val = 0f;
                string result = "";
                EntityPlayer eplr = args.Caller.Player.WorldData.EntityPlayer;
                SkillSystem instance = (SkillSystem)api.ModLoader.GetModSystem("Origins.Character.SkillSystem");

                foreach (Skill skill in instance.Skills)
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
        }

        private static void CreateDelCommand(ICoreServerAPI api)
        {
            IChatCommand del = api.ChatCommands.Create("del");
            del.RequiresPrivilege(Privilege.root);
            del.WithDescription("Deletes all Origin Skills from the caller's player data.");
            del.HandleWith(args => {
                string result = "";
                EntityPlayer eplr = args.Caller.Player.WorldData.EntityPlayer;
                SkillSystem instance = (SkillSystem)api.ModLoader.GetModSystem("Origins.Character.SkillSystem");

                foreach (Skill skill in instance.Skills)
                {
                    eplr.WatchedAttributes.RemoveAttribute("s_" + skill.Name);
                }

                return TextCommandResult.Success(result);
            });


            del.Validate(); // name, priv, desc, handler
        }
    }
}
