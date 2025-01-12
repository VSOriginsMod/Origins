using Newtonsoft.Json.Linq;
using Origins.Systems;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace Origins.Patches.Behaviors
{
    internal class BBFarmlandCropDebug : BlockBehavior, ICodePatch
    {

        static readonly string[] PatchedClasses =
        {
            "BlockFarmland",
        };

        public BBFarmlandCropDebug(Block block) : base(block)
        {
        }

        public override string GetPlacedBlockInfo(IWorldAccessor world, BlockPos pos, IPlayer forPlayer)
        {
            Block above = world.BlockAccessor.GetBlock(pos.Up());
            if (above == null || !above.Class.Equals("BlockCrop"))
            {
                return "Mutation Rate: UNKNOWN";
            }

            // BUG(chris): this breaks HUD, causing it to blink on some frames with only this in it
            if (above.GetBehavior<BBTransitiveAttribute>() is BBTransitiveAttribute behavior)
            {
                return behavior.GetPlacedBlockInfo(world, pos, forPlayer);
            }

            return "";
        }

        public static void ApplyPatch(ICoreAPI api)
        {
            if (api.Side != EnumAppSide.Server)
            {
                return;
            }

            foreach (Block block in api.World.Blocks)
            {
                // first two are necessary to make sure it exists, third is for a robust method of filtering
                if (block == null || block.Code == null || block.Class == null || block.EntityClass == null)
                {
                    continue;
                }

                //if (block.Code.PathStartsWith("farmland"))
                if (PatchedClasses.Contains(block.Class))
                {
                    //block behavior
                    BBFarmlandCropDebug behavior = new BBFarmlandCropDebug(block);

                    JsonObject properties = new JsonObject(new JObject());

                    behavior.Initialize(properties);

                    // since VSEssentials adds to both, we cannot vary from this practice
                    block.CollectibleBehaviors = block.CollectibleBehaviors.Append(behavior);
                    block.BlockBehaviors = block.BlockBehaviors.Append(behavior);


                    /*block entity behavior
                    BlockEntityBehaviorType behavior = new BlockEntityBehaviorType()
                    {
                        Name = "BBFarmlandCropDebug",
                    };

                    behavior.properties = new Vintagestory.API.Datastructures.JsonObject(new JObject());

                    behavior.properties.Token["foo"] = JToken.FromObject("bar");

                    block.BlockEntityBehaviors = block.BlockEntityBehaviors.Append(behavior);
                    */
                }
            }
        }

        public static void RegisterPatch(ICoreAPI api)
        {
            OriginsLogger.Debug(api, "[BBFarmlandCropDebug] Registering patch: BBFarmlandCropDebug");
            api.RegisterBlockBehaviorClass("BBFarmlandCropDebug", typeof(BBFarmlandCropDebug));
            /*block entity behavior
            api.RegisterBlockEntityBehaviorClass("BBFarmlandCropDebug", typeof(BBFarmlandCropDebug));
            */
        }
    }
}
