using Newtonsoft.Json.Linq;
using Origins.Systems;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;

namespace Origins.Patches.Behaviors
{
    internal class BBTransitiveAttribute : BlockBehavior
    {
        static readonly string attribute_list_name = "transitiveAttributes";
        static readonly string[] attr_list = new string[] { "mutationRate" };

        static readonly string[] PatchedClasses =
        {
            "BlockCrop",
        };

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

            block.Attributes ??= properties ?? new JsonObject(new JObject());

            collObj.Attributes.Token[attribute_list_name] = properties.Token[attribute_list_name];

            foreach (string attrKey in attr_list)
            {
                // needs default init values if none are given by properties
                collObj.Attributes.Token[attrKey] ??= JToken.FromObject(1.0f);
            }
        }

        public override void OnHeldAttackStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandHandling handHandling, ref EnumHandling handling)
        {
            OriginsLogger.Debug(byEntity.Api, "Attacking using a block with transitive properties!");

            PatchDebugger.PrintDebug(byEntity.Api, slot.Itemstack.ItemAttributes);

            base.OnHeldAttackStart(slot, byEntity, blockSel, entitySel, ref handHandling, ref handling);
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

                    properties.Token[attribute_list_name] = JToken.FromObject(new string[] { "yeild", "mutationRate" });

                    behavior.Initialize(properties);

                    // since VSEssentials adds to both, we cannot vary from this practice
                    block.CollectibleBehaviors = block.CollectibleBehaviors.Append(behavior);
                    block.BlockBehaviors = block.BlockBehaviors.Append(behavior);
                }
            }

        }

        public static void RegisterPatch(ICoreAPI api)
        {
            api.RegisterCollectibleBehaviorClass("BBTransitiveAttribute", typeof(BBTransitiveAttribute));
            api.RegisterBlockBehaviorClass("BBTransitiveAttribute", typeof(BBTransitiveAttribute));
        }
    }
}
