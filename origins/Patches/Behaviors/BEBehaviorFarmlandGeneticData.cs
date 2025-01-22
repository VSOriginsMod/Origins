using Origins.Systems.Horticulture;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace Origins.Patches.Behaviors
{
    internal class BEBehaviorFarmlandGeneticData : BlockEntityBehavior, ICodePatch
    {
        public BEBehaviorFarmlandGeneticData(BlockEntity blockentity) : base(blockentity)
        {
        }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            ITreeAttribute geneticData = Api.ModLoader.GetModSystem<HorticultureSystem>()?.GetAttributes(Pos.UpCopy());
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);
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
}
