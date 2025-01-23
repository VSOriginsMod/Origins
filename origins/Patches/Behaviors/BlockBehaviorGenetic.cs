using Newtonsoft.Json.Linq;
using Origins.Systems;
using System;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace Origins.Patches.Behaviors;

internal class BlockBehaviorGenetic : BlockBehavior, ICodePatch
{

    static readonly string attr_list_name = "genetic_attributes";
    static readonly string[] attr_list = new string[] { "mutation" };
    static readonly Random random = new Random();

    public BlockBehaviorGenetic(Block block) : base(block)
    {
    }

    /// <summary>
    /// Mostly used for manual initialization but also called when JSON patch applies this behavior.
    /// </summary>
    /// <param name="properties">will only have values when JSON patches apply this behavior</param>
    public override void Initialize(JsonObject properties)
    {
        base.Initialize(properties);

        // the code until the end of the foreach loop is for making sure collectible objects retain externally defined Attributes
        // ensures (transitive) attribute list is in block's 'Attributes'
        block.Attributes ??= properties ?? new JsonObject(new JObject());
        block.Attributes.Token[attr_list_name] ??= JToken.FromObject(attr_list);
    }

    // NOTE(chris): This does NOT run when using seeds
    public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack, ref EnumHandling handling)
    {
        OriginsLogger.Debug(world.Api, "[BlockBehaviorGenetic::DoPlaceBlock] placing {0} at {1}", block.Code, blockSel.Position.ToString());

        return base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack, ref handling);
    }

    // NOTE(chris): This _does_ run when using seeds
    public override void OnBlockPlaced(IWorldAccessor world, BlockPos blockPos, ref EnumHandling handling)
    {
        //if (world.Api.Side == EnumAppSide.Client)
        //{
        //    return;
        //}

        double attr = world.BlockAccessor.GetBlock(blockPos.DownCopy())?.GetBEBehavior<BEBehaviorFarmlandGeneticData>(blockPos.DownCopy())?.Mutation ?? -1.0d;

        OriginsLogger.Debug(world.Api, "[BlockBehaviorGenetic::OnBlockPlaced] {0} block placed at {1} with attribute {2}",
            block.Code, blockPos.ToString(), attr
        );
        base.OnBlockPlaced(world, blockPos, ref handling);
    }

    public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos, ref EnumHandling handling)
    {
        handling = EnumHandling.Handled;
        ItemStack stack = new ItemStack(block);
        var val = world.BlockAccessor.GetBlock(pos)?.GetBEBehavior<BEBehaviorFarmlandGeneticData>(pos.DownCopy())?.Mutation ?? -1.0d;

        stack.StackSize = 1;
        stack.Attributes.SetDouble("mutation", val);

        return stack;
    }

    /// <summary>
    /// This runs in BlockCrop as a BlockBehavior when a crop is broken: it is one of the first things to run.
    /// </summary>
    /// <param name="world"></param>
    /// <param name="pos"></param>
    /// <param name="byPlayer"></param>
    /// <param name="handling"></param>
    public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, ref EnumHandling handling)
    {
        //if (world.Api.Side == EnumAppSide.Client)
        //{
        //    return;
        //}


        BlockEntityFarmland farmland = world.Api.World.BlockAccessor.GetBlockEntity<BlockEntityFarmland>(pos.DownCopy());
        if (farmland == null)
        {
            return;
        }

        double geneticData = farmland.GetBehavior<BEBehaviorFarmlandGeneticData>().Mutation;

        foreach (BlockDropItemStack stack in block.Drops)
        {
            var resolvedStack = stack.ResolvedItemstack;
            if (resolvedStack.ItemAttributes.KeyExists(attr_list_name))
            {
                resolvedStack.Attributes.SetDouble("mutation", geneticData + Mutation());
            }
        }

        farmland.GetBehavior<BEBehaviorFarmlandGeneticData>().Mutation = 0;

        base.OnBlockBroken(world, pos, byPlayer, ref handling);
    }

    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
        dsc.AppendLine("Mutation: " + inSlot.Itemstack.Attributes.GetDouble(attr_list[0]));
    }

    [Obsolete]
    public override string GetPlacedBlockInfo(IWorldAccessor world, BlockPos pos, IPlayer forPlayer)
    {
        return "Mutation: " + world.Api.World.BlockAccessor
            .GetBlockEntity<BlockEntityFarmland>(pos.DownCopy())?
            .GetBehavior<BEBehaviorFarmlandGeneticData>()?.Mutation ?? "Unknown";
    }

    #region ICodePatch
    public static void ApplyPatch(ICoreAPI api)
    {
        if (api.Side != EnumAppSide.Server)
        {
            return;
        }

        foreach (Block block in api.World.Blocks)
        {
            // first two are necessary to make sure it exists, third is for a robust method of filtering
            if (block == null || block.Code == null || block.Class == null)
            {
                continue;
            }

            //if (block.Code.PathStartsWith("crop"))
            if (block is BlockCrop)
            {
                BlockBehaviorGenetic behavior = new BlockBehaviorGenetic(block);

                JsonObject properties = new JsonObject(new JObject());

                behavior.Initialize(properties);

                // since VSEssentials adds to both, we cannot vary from this practice
                block.CollectibleBehaviors = block.CollectibleBehaviors.Append(behavior);
                block.BlockBehaviors = block.BlockBehaviors.Append(behavior);
            }
        }
    }

    public static void RegisterPatch(ICoreAPI api)
    {
        api.RegisterCollectibleBehaviorClass("BlockBehaviorGenetic", typeof(BlockBehaviorGenetic));
        api.RegisterBlockBehaviorClass("BlockBehaviorGenetic", typeof(BlockBehaviorGenetic));
    }
    #endregion

    private static double Mutation()
    {
        // divide by 8
        // extract exponent
        // "multiply" by 1/8; bitshift exponent left 2
        //  _ : X EEE EEEE EEEE DDDD DDDD DDDD DDDD DDDD DDDD DDDD DDDD DDDD DDDD DDDD DDDD DDDD
        // <<2: E EEE EEEE EEDD DDDD
        // AND: 0 111 1111 1100
        // EXP: 0 EEE EEEE EE00

        // extract mantissa
        //  _ : X EEE EEEE EEEE DDDD DDDD DDDD DDDD DDDD DDDD DDDD DDDD DDDD DDDD DDDD DDDD DDDD
        // AND: 0 000 0000 0000 1111 ...
        // MAN: 0 000 0000 0000 DDDD ...

        // conjunction
        // EXP: 0 EEE EEEE EE00
        // MAN: 0 000 0000 0000 DDDD ...
        //
        // RES: X EEE EEEE EE00 DDDD ...

        // subtract 1/16 with 2's complement
        //  _ : X EEE EEEE EEEE DDDD DDDD DDDD DDDD DDDD DDDD DDDD DDDD DDDD DDDD DDDD DDDD DDDD
        // value to subtract:
        //  A :  0 111 1111 1011 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000
        // ~A :  1 000 0000 0100 1111 1111 1111 1111 1111 1111 1111 1111 1111 1111 1111 1111 1111
        // 
        // result:
        // ---> X 111 1111 1011 XXXX

        return (random.NextDouble() - 0.5d) / 8.0d;
    }
}
