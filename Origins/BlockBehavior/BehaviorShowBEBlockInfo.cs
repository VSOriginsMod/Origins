using Origins.Util;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Origins.GameContent;

internal class BlockBehaviorShowBEBlockInfo : BlockBehavior, IPatch
{
    public BlockBehaviorShowBEBlockInfo(Block block) : base(block)
    {
    }

    public override string GetPlacedBlockInfo(IWorldAccessor world, BlockPos pos, IPlayer forPlayer)
    {
        if (block.GetBEBehavior<BEBehaviorFarmlandGeneticData>(pos) is BEBehaviorFarmlandGeneticData behavior)
        {
            StringBuilder builder = new StringBuilder();
            behavior.GetBlockInfo(forPlayer, builder);
            return builder.ToString();
        }

        return "";
    }

    public static void RegisterPatch(ICoreAPI api)
    {
        api.RegisterCollectibleBehaviorClass("BlockBehaviorShowBEBlockInfo", typeof(BlockBehaviorShowBEBlockInfo));
        api.RegisterBlockBehaviorClass("BlockBehaviorShowBEBlockInfo", typeof(BlockBehaviorShowBEBlockInfo));
    }
}
