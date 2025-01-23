using Newtonsoft.Json.Linq;
using Origins.Util;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;

namespace Origins.GameContent;

internal class CollectibleBehaviorGenetic : CollectibleBehavior, ICodePatch
{
    static readonly string attr_list_name = "genetic_attributes";
    static readonly string[] attr_list = new string[] { "mutation" };

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

        collObj.Attributes ??= properties ?? new JsonObject(new JObject());

        // BUG(chris): not setting anything here
        //     probably should set attribute list in here
    }

    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
        dsc.AppendLine("Mutation Rate: " + inSlot.Itemstack.Attributes[attr_list[0]]);
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

            if (item.Code.BeginsWith("game", "seeds"))
            {
                CollectibleBehaviorGenetic behavior = new CollectibleBehaviorGenetic(item);

                // Just in case we're going first
                item.Attributes ??= new JsonObject(new JObject());

                // We just need the list so we can check ItemStack::ItemAttributes later
                item.Attributes.Token[attr_list_name] = JToken.FromObject(attr_list);

                // In code mods, this needs to be called manually
                behavior.Initialize(item.Attributes);

                item.CollectibleBehaviors = item.CollectibleBehaviors.Append(behavior);
            }
        }
    }

    public static void RegisterPatch(ICoreAPI api)
    {
        api.RegisterCollectibleBehaviorClass("CollectibleBehaviorGenetic", typeof(CollectibleBehaviorGenetic));
    }
}
