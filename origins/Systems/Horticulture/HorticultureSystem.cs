using Origins.Gui;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace Origins.Systems.Horticulture;

// TODO (chris): add permanence for "Genetics" hashtable
internal class HorticultureSystem : ModSystem
{
    /// <summary>
    /// Stores data by Position.
    /// </summary>
    Dictionary<BlockPos, double> GeneticsAttributes;

    public void SetAttributes(BlockPos blockPos, double attr)
    {
        GeneticsAttributes.Remove(blockPos);
        GeneticsAttributes.Add(blockPos, attr);
    }

    public double GetAttributes(BlockPos blockPos)
    {
        return GeneticsAttributes.GetValueOrDefault(blockPos, 1.0d);
    }

    public override void StartPre(ICoreAPI api)
    {
        GeneticsAttributes = new Dictionary<BlockPos, double>();
    }

    public override void Start(ICoreAPI api)
    {
        api.RegisterItemClass("ItemSeedAnalyzer", typeof(ItemSeedAnalyzer));
    }

    // This basically does what my modified VSSurvivalMod.dll does, but this doesn't work for no good reason :(
    //[HarmonyTranspiler]
    //[HarmonyPatch(typeof(ItemPlantableSeed), "OnHeldInteractStart")]
    //public static IEnumerable<CodeInstruction> OnHeldInteractStart(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    //{
    //    //Api.Logger.Debug("Harmony patch applied");
    //    var editor = new CodeMatcher(instructions, generator);
    //    try
    //    {
    //        //editor
    //        editor
    //            .MatchStartForward(new CodeMatch((CodeInstruction instr) => instr.opcode.Equals(OpCodes.Callvirt) && instr.operand.ToString().Equals("Boolean TryPlant(Vintagestory.API.Common.Block)")));
    //        Console.WriteLine("[ASM] matched at index:" + editor.Pos);
    //        var newInstructions = editor
    //            .Advance(3)
    //            //load args for block.OnBlockStartInteract
    //            /* ldloc.3
    //             * ldarg.2
    //             * ldfld class [VintagestoryAPI]Vintagestory.API.Common.IWorldAccessor [VintagestoryAPI]VintageStory.API.Common.Entities.Entity::World
    //             * ldloc.s V_4 (4)
    //             * ldarg.3
    //             */
    //            .InsertAndAdvance(
    //                new CodeInstruction(OpCodes.Ldloc_3), // stack cropBlock
    //                new CodeInstruction(OpCodes.Ldarg_2), // stack byEntity
    //                                                      // replace previous element on stack with value: IWorldAccessor Entity::World
    //                new CodeInstruction(OpCodes.Ldfld, AccessTools.DeclaredField("Vintagestory.API.Common.Entities.Entity:World")),// "class [VintagestoryAPI]Vintagestory.API.Common.IWorldAccessor [VintagestoryAPI]VintageStory.API.Common.Entities.Entity::World"),
    //                new CodeInstruction(OpCodes.Ldloc_S, "4"), // stack byPlayer
    //                new CodeInstruction(OpCodes.Ldarg_3), // stack blockSel
    //                // stack entitysel
    //                // stack 
    //                new CodeInstruction(OpCodes.Callvirt, AccessTools.DeclaredMethod(typeof(Block), "OnBlockInteractStart")),  //  "instance bool [VintagestoryAPI]Vintagestory.API.Common.Block::OnBlockInteractStart(class [VintagestoryAPI]Vintagestory.API.Common.IWorldAccessor, class [VintagestoryAPI]Vintagestory.API.Common.IPlayer, class [VintagestoryAPI]Vintagestory.API.Common.BlockSelection)"),
    //                new CodeInstruction(OpCodes.Pop)
    //                )
    //            /* callvirt instance bool [VintagestoryAPI]Vintagestory.API.Common.Block::OnBlockInteractStart(class [VintagestoryAPI]Vintagestory.API.Common.IWorldAccessor, class [VintagestoryAPI]Vintagestory.API.Common.IPlayer, class [VintagestoryAPI]Vintagestory.API.Common.BlockSelection)
    //             * pop
    //             */
    //            .InstructionEnumeration();
    //        Console.WriteLine("[ASM] position after writing all instructions: " + editor.Pos);
    //        int i = 0;
    //        Console.WriteLine("[ASM] after writing all instructions:\n" + newInstructions.Join((instr) =>
    //        {
    //            return "[" + i++ + "]" + instr.opcode.Name + "  " + instr.operand?.ToString();
    //        }, "\n"));

    //        return newInstructions;
    //    }
    //    catch (Exception e)
    //    {
    //        Console.Error.WriteLine("Hit Exception!");
    //        Console.Error.WriteLine(e.ToString());
    //        throw new Exception("still not working :(");
    //    }
    //}

    public override void StartClientSide(ICoreClientAPI api)
    {
        SeedAnalyzer = new HudSeedAnalyzer(api);
        // TODO (chris): way down the line, "wearable" has related keybind (like glasses)
        /*
        api.Input.RegisterHotKey(
            "Skill Interface",
            "Opens up the Skills GUI",
            GlKeys.O,
            HotkeyType.GUIOrOtherControls);

        api.Input.SetHotKeyHandler("Skill Interface", ToggleGUI);//*/
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        base.StartServerSide(api);

        this.sapi = api;
        Player.sapi = api;

        api.Event.PlayerJoin += Player.Event_PlayerJoin;
    }

    internal class Player
    {
        static readonly string[] attr_list = new string[] { "mutation" };
        private static Player[] state = new Player[1];
        internal static ICoreServerAPI sapi;
        internal static void Event_PlayerJoin(IServerPlayer byPlayer)
        {
            state = state.Append(new Player(byPlayer));
        }


        private IServerPlayer player;

        internal Player(IServerPlayer player)
        {
            this.player = player;
            this.player.InWorldAction += Player_InWorldAction;
        }

        internal void Player_InWorldAction(EnumEntityAction action, bool on, ref EnumHandling handled)
        {
            switch (action)
            {
                case EnumEntityAction.None:
                    break;
                case EnumEntityAction.Forward:
                    break;
                case EnumEntityAction.Backward:
                    break;
                case EnumEntityAction.Left:
                    break;
                case EnumEntityAction.Right:
                    break;
                case EnumEntityAction.Jump:
                    break;
                case EnumEntityAction.Sneak:
                    break;
                case EnumEntityAction.Sprint:
                    break;
                case EnumEntityAction.Glide:
                    break;
                case EnumEntityAction.FloorSit:
                    break;
                case EnumEntityAction.LeftMouseDown:
                    break;
                case EnumEntityAction.RightMouseDown:
                    OriginsLogger.Debug(player.Entity.Api, "[Player::Player_InWorldAction] Right clicked!");
                    CheckSeedPlanting();
                    break;
                case EnumEntityAction.Up:
                    break;
                case EnumEntityAction.Down:
                    break;
                case EnumEntityAction.CtrlKey:
                    break;
                case EnumEntityAction.ShiftKey:
                    break;
                case EnumEntityAction.InWorldLeftMouseDown:
                    break;
                case EnumEntityAction.InWorldRightMouseDown:
                    break;
                default:
                    break;
            }
        }

        private void CheckSeedPlanting()
        {
            ItemStack activeStack = player.InventoryManager.ActiveHotbarSlot.Itemstack;
            if (activeStack?.Item == null || !activeStack.Item.Code.BeginsWith("game", "seeds"))
            {
                return;
            }


            if (player.CurrentBlockSelection == null || !player.CurrentBlockSelection.Block.Code.PathStartsWith("farmland"))
            {
                return;
            }

            BEBehaviorFarmlandGeneticData befarmland = player.CurrentBlockSelection.Block.GetBEBehavior<BEBehaviorFarmlandGeneticData>(player.CurrentBlockSelection.Position);

            if (!((BlockEntityFarmland)befarmland.Blockentity).CanPlant())
            {
                return;
            }

            befarmland.Mutation = activeStack.Attributes.GetDouble(attr_list[0]);
            OriginsLogger.Debug(player.Entity.Api,
                "[Player::CheckSeedPlanting] Successfully saved genetic data: {0}",
                player.CurrentBlockSelection.Block.GetBEBehavior<BEBehaviorFarmlandGeneticData>(player.CurrentBlockSelection.Position).Mutation
            );
        }
    }

    HudSeedAnalyzer SeedAnalyzer;
    private ICoreServerAPI sapi;

    [Obsolete("This is just on the back burner right now")]
    public HudSeedAnalyzer GetHudSeedAnalyzer() => SeedAnalyzer;
}
