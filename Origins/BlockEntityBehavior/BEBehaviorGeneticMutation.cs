using Origins.Util;
using System;
using System.Linq;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace Origins.GameContent;

internal class BEBehaviorFarmlandGeneticData : BlockEntityBehavior, ICodePatch
{
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
        base.ToTreeAttributes(tree);
        tree.SetDouble("mutation", mutation);
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
    {
        base.FromTreeAttributes(tree, worldAccessForResolve);
        mutation = tree.GetDouble("mutation");
    }

    #region ICodePatch
    public static void ApplyPatch(ICoreAPI api)
    {
        foreach (var block in api.World.Blocks)
        {
            if (block == null || block.Code == null || block.Class == null)
            {
                continue;
            }
            if (block is BlockFarmland)
            {
                block.BlockEntityBehaviors = block.BlockEntityBehaviors.Append(new BlockEntityBehaviorType()
                {
                    Name = "BEBehaviorFarmlandGeneticData",
                    properties = null
                });
            }
        }
    }

    public static void RegisterPatch(ICoreAPI api)
    {
        api.RegisterBlockEntityBehaviorClass("BEBehaviorFarmlandGeneticData", typeof(BEBehaviorFarmlandGeneticData));
    }
    #endregion
}
