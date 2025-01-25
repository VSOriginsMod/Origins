using Origins.Util;
using System;
using System.Linq;
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
public class BlockBehaviorMutableGenes : BlockBehavior, IPatch
{
    static readonly string AttributeName = "genes";
    static readonly Random random = new Random();

    /// <summary>
    /// Keys hold gene names that map to Values of TreeAttribute with "default"
    /// keys containing default values.
    /// </summary>
    // NOTE(chris): this may be redunant, I just want to make sure default values exist
    private TreeAttribute genes;

    public TreeAttribute Genes { get => genes; set => genes = value; }

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
        if (!genes.GetType().IsEquivalentTo(typeof(TreeAttribute)))
        {
            throw new ContextMarshalException(
                "Unable to parse TreeAttribute from 'properties.genes'; 'genes' must be an object!",
                new FormatException(propertiesAtString)
            );
        }

        block.Attributes.Token[AttributeName] = properties.Token[AttributeName];
        this.genes = (TreeAttribute)genes.GetValue();
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
        BEBehaviorFarmlandGeneticData bebfarmland = world.BlockAccessor
            .GetBlockEntity<BlockEntityFarmland>(blockPos.DownCopy())?
            .GetBehavior<BEBehaviorFarmlandGeneticData>();

        if (null == bebfarmland)
        {
            return;
        }

        double mut = bebfarmland.Mutation;
        OriginsLogger.Debug(world.Api, "[BlockBehaviorMutableGenes::OnBlockPlaced] {0} block placed at {1} with attribute {2}",
            block.Code, blockPos.ToString(), mut
        );

        // TODO(chris): set genetic data
        // needs to be done once we know what crop is planted
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
        if (null == genes)
        {
            return;
        }

        BlockEntityFarmland farmland = world.Api.World.BlockAccessor.GetBlockEntity<BlockEntityFarmland>(pos.DownCopy());
        if (farmland == null)
        {
            return;
        }

        BEBehaviorFarmlandGeneticData bebfarmland = farmland.GetBehavior<BEBehaviorFarmlandGeneticData>();
        double mut = bebfarmland.Mutation;

        if (0 == bebfarmland.Genes.ToArray().Length)
        {
            bebfarmland.Genes.MergeTree(genes);
        }

        foreach (BlockDropItemStack stack in block.Drops
        .Where(
            s => null != s.ResolvedItemstack.ItemAttributes &&
            s.ResolvedItemstack.ItemAttributes.KeyExists(AttributeName)
            )
        )
        {
            stack.ResolvedItemstack.Attributes
                .SetDouble("mutation", mut + Mutation());

            bebfarmland.Mutate();

            stack.ResolvedItemstack.Attributes
                .GetOrAddTreeAttribute(AttributeName)
                .MergeTree(bebfarmland.Genes);
        }

        bebfarmland.Mutation = 0;
        bebfarmland.Genes.MergeTree(genes);

        base.OnBlockBroken(world, pos, byPlayer, ref handling);
    }

    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

        if (genes == null)
        {
            return;
        }

        foreach (var key in genes.Keys)
        {
            dsc.AppendLine(
                string.Format(
                    "{0}: {1}",
                    key,
                    inSlot.Itemstack.Attributes.GetTreeAttribute(AttributeName)?.GetDouble(key) ?? -128.0d
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
