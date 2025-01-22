using Newtonsoft.Json.Linq;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using static Origins.Systems.Horticulture.HorticultureSystem;

namespace Origins.Patches.Behaviors
{
    internal class CollectibleBehaviorGenetic : CollectibleBehavior, ICodePatch
    {
        static readonly string[] PatchedClasses = new string[] { "ItemPlantableSeed" };
        static readonly string[] attr_list = new string[] { "mutationRate" };
        static readonly string attr_list_name = "genetic_attributes";

        static ICoreAPI api;

        public CollectibleBehaviorGenetic(CollectibleObject collObj) : base(collObj)
        {
        }

        /// <summary>
        /// Mostly used for manual initialization but also called when JSON patch applies this behavior.
        /// </summary>
        /// <param name="properties">will only have values when JSON patches apply this behavior</param>
        public override void Initialize(JsonObject properties)
        {
            base.Initialize(properties);

            //OriginsLogger.Debug(api, propertiesAtString);

            collObj.Attributes ??= new JsonObject(new JObject());
            collObj.Attributes.Token[attr_list_name] ??= properties.Token[attr_list_name];

        }

        private static bool StackHasAttribute(ItemSlot slot, GeneticData attr)
        {
            return slot.Itemstack.Attributes.HasAttribute(attr.name);
        }

        // supported types: decimal, int, string
        // have to set default values by type here, if they don't exist, so get ready for some nests!
        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

            if (inSlot.Itemstack.ItemAttributes.Exists)
            {
                // ItemAttributes to iterate over attributes the ItemStack should have
                // Format: attr_name: attr_val
                foreach (var attr in inSlot.Itemstack.ItemAttributes[attr_list_name].AsArray<GeneticData>())
                {
                    //var attr = el.AsObject<GeneticData>();
                    dsc.Append(attr.name).Append(": ");

                    if (!StackHasAttribute(inSlot, attr))
                    {
                        inSlot.Itemstack.Attributes.SetDouble(attr.name, 1.0d);
                        //inSlot.Itemstack.Attributes.SetDouble(attr.name, attr.init);
                    }

                    dsc.AppendLine(inSlot.Itemstack.Attributes.GetAsString(attr.name));
                }
            }
        }

        public static void RegisterPatch(ICoreAPI api)
        {
            CollectibleBehaviorGenetic.api = api;

            api.RegisterCollectibleBehaviorClass("CollectibleBehaviorGenetic", typeof(CollectibleBehaviorGenetic));
        }
    }
}
