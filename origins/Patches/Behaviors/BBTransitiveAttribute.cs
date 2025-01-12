using Newtonsoft.Json.Linq;
using Origins.Systems;
using System.Linq;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace Origins.Patches.Behaviors
{
    internal class BBTransitiveAttribute : BlockBehavior, ICodePatch
    {
        static readonly string attr_list_name = "transitiveAttributes";
        static readonly string[] attr_list = new string[] { "mutationRate" };

        static readonly string[] PatchedClasses =
        {
            "BlockCrop",
        };

        static ICoreAPI api;
        private float mutation = 1.0f;

        public BBTransitiveAttribute(Block block) : base(block)
        {
        }

        /// <summary>
        /// Mostly used for manual initialization but also called when JSON patch applies this behavior.
        /// </summary>
        /// <param name="properties">will only have values when JSON patches apply this behavior</param>
        public override void Initialize(JsonObject properties)
        {
            base.Initialize(properties);

            OriginsLogger.Debug(api, "[BBTransitiveAttribute] Initializing " + block.Code);

            if (!propertiesAtString.Equals("{}"))
                api.Logger.Debug("Properties:\n{0}", propertiesAtString);

            // the code until the end of the foreach loop is for making sure collectible objects retain externally defined Attributes
            // ensures (transitive) attribute list is in block's 'Attributes'
            block.Attributes ??= properties ?? new JsonObject(new JObject());
            block.Attributes.Token[attr_list_name] ??= JToken.FromObject(attr_list);

            foreach (string attrKey in attr_list)
            {
                // needs default init values if none are given by properties
                block.Attributes.Token[attrKey] ??= JToken.FromObject(mutation);
            }
        }

        // HACK(chris): remove when better way exists
        public override void OnHeldAttackStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandHandling handHandling, ref EnumHandling handling)
        {
            OriginsLogger.Debug(byEntity.Api, "Attacking using a block with transitive properties!");

            PatchDebugger.PrintDebug(byEntity.Api, slot.Itemstack.ItemAttributes);

            base.OnHeldAttackStart(slot, byEntity, blockSel, entitySel, ref handHandling, ref handling);
        }

        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
            dsc.AppendLine("Mutation Rate: " + mutation);
        }

        public override string GetPlacedBlockInfo(IWorldAccessor world, BlockPos pos, IPlayer forPlayer)
        {
            return "Mutation Rate: " + mutation;
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
                if (block == null || block.Code == null || block.Class == null)
                {
                    continue;
                }

                //if (block.Code.PathStartsWith("crop"))
                if (PatchedClasses.Contains(block.Class))
                {
                    BBTransitiveAttribute behavior = new BBTransitiveAttribute(block);

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
            BBTransitiveAttribute.api = api;

            OriginsLogger.Debug(api, "[BBTransitiveAttribute] Registering patch: BBTransitiveAttribute");

            api.RegisterCollectibleBehaviorClass("BBTransitiveAttribute", typeof(BBTransitiveAttribute));
            api.RegisterBlockBehaviorClass("BBTransitiveAttribute", typeof(BBTransitiveAttribute));
        }
    }
}
