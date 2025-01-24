using Origins.Util;
using System;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace Origins.GameContent;

/// <summary>
/// WARN(chris): Currently only intended for BlockCrop!
/// 
/// Behavior indicating a block has mutable genes to keep track of.
/// This will be used to transfer data to store genetic data in a corresponding
///   BlockEntity and slightly mutate that data for propogation when the block
///   is harvasted.
/// </summary>
internal class BlockBehaviorMutableGenes : BlockBehavior, IPatch
{
    static readonly string AttributeName = "genes";
    static readonly Random random = new Random();

    /// <summary>
    /// elements hold gene name as first key and default value as first key's value
    /// </summary>
    // NOTE(chris): this may be redunant, I just want to make sure default values exist
    private TreeAttribute[] genes;

    public BlockBehaviorMutableGenes(Block block) : base(block)
    {
    }

    /// <summary>
    /// Mostly used for manual initialization but also called when JSON patch applies this behavior.
    /// </summary>
    /// <param name="properties">will only have values when JSON patches apply this behavior</param>
    public override void Initialize(JsonObject properties)
    {
        if (null == block.Attributes)
        {
            return;
        }

        base.Initialize(properties);

        var genes = properties[AttributeName].ToAttribute();

        if (!genes.GetType().IsEquivalentTo(typeof(TreeArrayAttribute)))
        {
            throw new ContextMarshalException(
                "Unable to parse TreeArrayAttribute from 'properties.genes'; 'genes' must be an array!",
                new FormatException(propertiesAtString)
            );
        }

        block.Attributes.Token[AttributeName] = properties.Token[AttributeName];
        this.genes = (TreeAttribute[])genes.GetValue();
    }

    // NOTE(chris): This does NOT run when using seeds
    public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack, ref EnumHandling handling)
    {
        OriginsLogger.Debug(world.Api, "[BlockBehaviorMutableGenes::DoPlaceBlock] placing {0} at {1}", block.Code, blockSel.Position.ToString());

        return base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack, ref handling);
    }

    // NOTE(chris): This _does_ run when using seeds
    public override void OnBlockPlaced(IWorldAccessor world, BlockPos blockPos, ref EnumHandling handling)
    {
        double attr = world.BlockAccessor
            .GetBlock(blockPos.DownCopy())?
            .GetBEBehavior<BEBehaviorFarmlandGeneticData>(blockPos.DownCopy())?
            .Mutation ?? -100;

        OriginsLogger.Debug(world.Api, "[BlockBehaviorMutableGenes::OnBlockPlaced] {0} block placed at {1} with attribute {2}",
            block.Code, blockPos.ToString(), attr
        );
        base.OnBlockPlaced(world, blockPos, ref handling);
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
        BlockEntityFarmland farmland = world.Api.World.BlockAccessor.GetBlockEntity<BlockEntityFarmland>(pos.DownCopy());
        if (farmland == null)
        {
            return;
        }

        double geneticData = farmland.GetBehavior<BEBehaviorFarmlandGeneticData>().Mutation;

        foreach (BlockDropItemStack stack in block.Drops)
        {
            var resolvedStack = stack.ResolvedItemstack;
            if (null != resolvedStack.ItemAttributes && resolvedStack.ItemAttributes.KeyExists(AttributeName))
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

        if (genes == null)
        {
            return;
        }

        foreach (var attr in genes)
        {
            dsc.AppendLine(
                string.Format(
                    "{0}: {1}",
                    attr.Keys[0],
                    inSlot.Itemstack.Attributes.GetDouble(attr.Keys[0])
                )
            );
        }
    }

    public static void RegisterPatch(ICoreAPI api)
    {
        api.RegisterCollectibleBehaviorClass("BlockBehaviorMutableGenes", typeof(BlockBehaviorMutableGenes));
        api.RegisterBlockBehaviorClass("BlockBehaviorMutableGenes", typeof(BlockBehaviorMutableGenes));
    }

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
