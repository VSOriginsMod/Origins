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
    /// Keys hold gene names that map to Values of TreeAttribute with "default"
    /// keys containing default values.
    /// </summary>
    // NOTE(chris): this may be redunant, I just want to make sure default values exist
    private TreeAttribute genes;

    public TreeAttribute Genes { get => genes; set => genes = value; }

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

        if (!genes.GetType().IsEquivalentTo(typeof(TreeAttribute)))
        {
            throw new ContextMarshalException(
                "Unable to parse TreeArrayAttribute from 'properties.genes'; 'genes' must be an array!",
                new FormatException(propertiesAtString)
            );
        }

        collObj.Attributes.Token[AttributeName] = properties.Token[AttributeName];
        this.genes = (TreeAttribute)genes.GetValue();
    }

    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

        if (genes == null)
        {
            return;
        }

        var attrs = inSlot.Itemstack.Attributes;
        var stackGenes = attrs.GetTreeAttribute(AttributeName);

        // use default vals if stack not instantiated
        if (null == stackGenes)
        {
            attrs = (TreeAttribute)inSlot.Itemstack.ItemAttributes.ToAttribute();
            stackGenes = attrs.GetTreeAttribute(AttributeName);
        }

        foreach (var attrKey in genes.Keys)
        {
            var stackGene = stackGenes.GetTreeAttribute(attrKey);
            double attrVal = -128;
            if (stackGene.HasAttribute("value"))
            {
                attrVal = stackGene.GetDouble("value");
            }
            else if (stackGene.HasAttribute("default"))
            {
                attrVal = stackGene.GetDouble("default");
            }

            dsc.AppendLine(
                string.Format(
                    "{0}: {1}",
                    attrKey, Math.Round(attrVal, 2)
                )
            );
        }
    }

    public static void RegisterPatch(ICoreAPI api)
    {
        api.RegisterCollectibleBehaviorClass("CollectibleBehaviorGenetic", typeof(CollectibleBehaviorGenetic));
    }
}
