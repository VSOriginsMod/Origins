using Newtonsoft.Json.Linq;
using Origins.Util;
using System;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Origins.GameContent;

internal class CollectibleBehaviorGenetic : CollectibleBehavior, IPatch
{
    static readonly string AttributeName = "genes";

    /// <summary>
    /// elements hold gene name as first key and default value as first key's value
    /// </summary>
    // NOTE(chris): this may be redunant, I just want to make sure default values exist
    private TreeAttribute[] genes;

    public CollectibleBehaviorGenetic(CollectibleObject collObj) : base(collObj)
    {
    }

    /// <summary>
    /// Mostly used for manual initialization but also called when JSON patch applies this behavior.
    /// </summary>
    /// <param name="properties">will only have values when JSON patches apply this behavior</param>
    public override void Initialize(JsonObject properties)
    {
        collObj.Attributes ??= new JsonObject(new JObject());

        base.Initialize(properties);

        var genes = properties[AttributeName].ToAttribute();

        if (!genes.GetType().IsEquivalentTo(typeof(TreeArrayAttribute)))
        {
            throw new ContextMarshalException(
                "Unable to parse TreeArrayAttribute from 'properties.genes'; 'genes' must be an array!",
                new FormatException(propertiesAtString)
            );
        }

        collObj.Attributes.Token[AttributeName] = properties.Token[AttributeName];
        this.genes = (TreeAttribute[])genes.GetValue();
    }

    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

        if (genes == null)
        {
            return;
        }

        foreach (var attr in genes)
        {
            dsc.AppendLine(
                string.Format(
                    "{0}: {1}",
                    attr.Keys[0],
                    inSlot.Itemstack.Attributes.GetDouble(attr.Keys[0])
                )
            );
        }
    }

    public static void RegisterPatch(ICoreAPI api)
    {
        api.RegisterCollectibleBehaviorClass("CollectibleBehaviorGenetic", typeof(CollectibleBehaviorGenetic));
    }
}
