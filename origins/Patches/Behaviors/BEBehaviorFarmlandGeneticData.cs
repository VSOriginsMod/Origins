using Origins.Patches;
using Origins.Systems;
using System;
using System.Linq;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

internal class BEBehaviorFarmlandGeneticData : BlockEntityBehavior, ICodePatch
{
    static readonly string attr_list_name = "genetic_attributes";
    static readonly string[] attr_list = new string[] { "mutation" };

    private double mutation;

    internal double Mutation
    {
        get => mutation;
        set
        {
            mutation = value;
            Blockentity.MarkDirty();
            OriginsLogger.Debug(Api, "[BEBehaviorFarmlandGeneticData] setting mutation to {0}", mutation);
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
        //double geneticData = Api.ModLoader.GetModSystem<HorticultureSystem>()?.GetAttributes(Pos.UpCopy()) ?? 0.0d;
        tree.SetDouble("mutation", mutation);
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
    {
        base.FromTreeAttributes(tree, worldAccessForResolve);
        mutation = tree.GetDouble("mutation");
    }

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
}
