using Newtonsoft.Json.Linq;
using Origins.Systems;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;

namespace Origins.Patches.Behaviors
{
    internal class CBTransitiveAttribute : CollectibleBehavior, ICodePatch
    {
        static readonly string[] PatchedClasses = new string[] { "ItemPlantableSeed" };
        static readonly string[] attr_list = new string[] { "mutationRate" };
        static readonly string attr_list_name = "transitiveAttributes";

        static ICoreAPI api;

        public CBTransitiveAttribute(CollectibleObject collObj) : base(collObj)
        {
        }

        /// <summary>
        /// Mostly used for manual initialization but also called when JSON patch applies this behavior.
        /// </summary>
        /// <param name="properties">will only have values when JSON patches apply this behavior</param>
        public override void Initialize(JsonObject properties)
        {
            base.Initialize(properties);

            OriginsLogger.Debug(api, "[CBTransitiveAttribute] Initializing");

            // the code until the end of the foreach loop is for making sure collectible objects retain externally defined Attributes
            // ensures (transitive) attribute list is in collObj's 'Attributes'
            collObj.Attributes ??= properties ?? new JsonObject(new JObject());
            collObj.Attributes.Token[attr_list_name] ??= JToken.FromObject(attr_list);

            foreach (string attrKey in attr_list)
            {
                // needs default init values if none are given by properties
                collObj.Attributes.Token[attrKey] ??= JToken.FromObject(1.0f);
            }
        }

        public override void OnHeldAttackStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandHandling handHandling, ref EnumHandling handling)
        {
            OriginsLogger.Debug(byEntity.Api, "Attacking using an item with transitive properties!");

            PatchDebugger.PrintDebug(byEntity.Api, slot.Itemstack.ItemAttributes);

            base.OnHeldAttackStart(slot, byEntity, blockSel, entitySel, ref handHandling, ref handling);
        }

        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
        {
            //ItemPlantableSeed
        }


        public static void ApplyPatch(ICoreAPI api)
        {
            if (api.Side != EnumAppSide.Server)
            {
                return;
            }

            foreach (CollectibleObject item in api.World.Collectibles)
            {
                // first two are necessary to make sure it exists, third is for a robust method of filtering
                if (item == null || item.Code == null || item.Class == null)
                {
                    continue;
                }

                if (PatchedClasses.Contains(item.Class))
                {
                    CBTransitiveAttribute behavior = new CBTransitiveAttribute(item);

                    JsonObject properties = new JsonObject(new JObject());

                    behavior.Initialize(properties);

                    item.CollectibleBehaviors = item.CollectibleBehaviors.Append(behavior);
                }
            }
        }

        public static void RegisterPatch(ICoreAPI api)
        {
            CBTransitiveAttribute.api = api;

            OriginsLogger.Debug(api, "[CBTransitiveAttribute] Registering patch: CBTransitiveAttribute");

            api.RegisterCollectibleBehaviorClass("CBTransitiveAttribute", typeof(CBTransitiveAttribute));
        }
    }
}
