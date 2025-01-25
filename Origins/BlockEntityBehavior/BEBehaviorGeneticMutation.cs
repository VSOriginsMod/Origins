using Origins.Util;
using System;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace Origins.GameContent;

internal class BEBehaviorFarmlandGeneticData : BlockEntityBehavior, IPatch
{
    static readonly string AttributeName = "genes";
    static readonly Random random = new Random();


    /// <summary>
    /// Only a double right now because it needs to remain synchronized.
    /// </summary>
    /// Ideally this should be a SyncedTreeAttribute.
    private double mutation;

    /// <summary>
    /// Synchronized
    /// </summary>
    internal double Mutation
    {
        get => mutation;
        set
        {
            mutation = value;
            Blockentity.MarkDirty();
        }
    }

    /// <summary>
    /// Assumed to only hold DoubleAttributes
    /// </summary>
    /// This is assumed in ToTreeAttributes & FromTreeAttributes.
    /// It is also assumed at every reference.
    internal SyncedTreeAttribute Genes;

    /// <summary>
    /// redundant because the only thing in Genes is what the crop puts in there
    /// </summary>
    internal BlockCrop crop;

    public BEBehaviorFarmlandGeneticData(BlockEntity blockentity) : base(blockentity)
    {
    }

    public override void Initialize(ICoreAPI api, JsonObject properties)
    {
        base.Initialize(api, properties);
    }

    public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
    {
        dsc.AppendFormat("Mutation: {0}", Math.Round(mutation, 2));
    }

    public override void ToTreeAttributes(ITreeAttribute tree)
    {
        tree.SetDouble("mutation", mutation);

        if (null == crop)
        {
            return;
        }

        // NOTE(chris): use until unexpected behavior
        //     should be an attribute tree full of TreeAttributes
        //     each TreeAttribute should have a default
        tree.GetOrAddTreeAttribute(AttributeName).MergeTree(Genes);
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
    {
        mutation = tree.GetDouble("mutation");

        Genes = new SyncedTreeAttribute();

        var genes = tree.GetTreeAttribute(AttributeName);
        if (null == genes)
        {
            return;
        }

        Genes.MergeTree(genes);
    }

    public static void RegisterPatch(ICoreAPI api)
    {
        api.RegisterBlockEntityBehaviorClass("BEBehaviorFarmlandGeneticData", typeof(BEBehaviorFarmlandGeneticData));
    }

    internal void Mutate()
    {
        // for each gene

        foreach (var kvmap in Genes)
        {
            TreeAttribute attribute = (TreeAttribute)kvmap.Value.GetValue();
            if (null == attribute)
            {
                // BUG(chris): this is an error state
                continue;
            }

            // just in case we're dealing with a default
            // while "default" and "value" existence should be a disjunction, I don't trust
            DoubleAttribute val = (DoubleAttribute)attribute.GetAttribute("default");

            //   if "default" key exists, delete it
            attribute.RemoveAttribute("default");
            //   if "value" key exists, store it locally
            // grab value if exists
            if (attribute.HasAttribute("value"))
            {
                val = (DoubleAttribute)attribute.GetAttribute("value");
            }
            //   set "value" key to local copy + mutation value
            // BP: check to see if attribute["value"] changes or needs to be done manually
            val.value = val.value + DblMutation();

            attribute.SetAttribute("value", val);
        }
        return;
    }

    private static double DblMutation()
    {
        return (random.NextDouble() - 0.5d) / 8.0d;
    }
}
