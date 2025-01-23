using Newtonsoft.Json.Linq;
using System.Linq;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace Origins.Patches.Behaviors
{
    internal class BlockBehaviorFarmlandGeneticData : BlockBehavior, ICodePatch
    {
        public BlockBehaviorFarmlandGeneticData(Block block) : base(block)
        {
        }

        public override string GetPlacedBlockInfo(IWorldAccessor world, BlockPos pos, IPlayer forPlayer)
        {
            //if (world.Api.Side == EnumAppSide.Client)
            //{
            //    return "";
            //}

            // BUG(chris): Sidded error: behavior.Mutation is not sync'd...
            if (block.GetBEBehavior<BEBehaviorFarmlandGeneticData>(pos) is BEBehaviorFarmlandGeneticData behavior)
            {
                StringBuilder builder = new StringBuilder();
                behavior.GetBlockInfo(forPlayer, builder);
                return builder.ToString();
                //return "Mutation: " + System.Math.Round(behavior.Mutation, 2);
            }

            return "";
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


                if (block is BlockFarmland)
                {
                    BlockBehaviorFarmlandGeneticData behavior = new BlockBehaviorFarmlandGeneticData(block);

                    behavior.Initialize(new JsonObject(new JObject()));

                    // since VSEssentials adds to both, we cannot vary from this practice
                    block.CollectibleBehaviors = block.CollectibleBehaviors.Append(behavior);
                    block.BlockBehaviors = block.BlockBehaviors.Append(behavior);
                }
            }
        }

        public static void RegisterPatch(ICoreAPI api)
        {
            api.RegisterCollectibleBehaviorClass("BlockBehaviorFarmlandGeneticData", typeof(BlockBehaviorFarmlandGeneticData));
            api.RegisterBlockBehaviorClass("BlockBehaviorFarmlandGeneticData", typeof(BlockBehaviorFarmlandGeneticData));
        }
        #endregion
    }
}
