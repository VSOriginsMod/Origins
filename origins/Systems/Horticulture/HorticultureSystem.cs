using HarmonyLib;
using Origins.Gui;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace Origins.Systems.Horticulture
{
    // TODO (chris): add permanence for "Genetics" hashtable
    [HarmonyPatch]
    internal class HorticultureSystem : ModSystem
    {
        /// <summary>
        /// Stores data by Position.
        /// </summary>
        Dictionary<BlockPos, double> GeneticsAttributes;
        Harmony harmony;

        public void SetAttributes(BlockPos blockPos, double attr)
        {
            if (GeneticsAttributes.ContainsKey(blockPos))
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
            if (!Harmony.HasAnyPatches("origins"))
            {
                api.Logger.Debug("[origins] harmony patching");
                Harmony.DEBUG = true;
                harmony = new Harmony("origins");
                harmony.PatchAll();
            }
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
        }

        HudSeedAnalyzer SeedAnalyzer;
        [Obsolete("This is just on the back burner right now")]
        public HudSeedAnalyzer GetHudSeedAnalyzer() => SeedAnalyzer;
    }
}
