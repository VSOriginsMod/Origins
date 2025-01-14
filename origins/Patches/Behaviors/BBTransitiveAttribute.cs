using Newtonsoft.Json.Linq;
using Origins.Systems;
using System;
using System.Collections.Generic;
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
        static Random random = new Random();
        static double mu = 0.5;
        static double sigma = Math.ReciprocalEstimate(3.92d);
        static NormalDistribution normalDistribution = GetNormalDistribution(mu, sigma);

        public class TransitiveProps : CropBehavior
        {
            internal BlockPos pos = null;
            internal double mutation = 0.0f;
            internal ItemStack stack;

            public TransitiveProps(Block block) : base(block)
            {
            }

            internal void OnBlockPlaced()
            {

            }
        };


        private double mutation = 1.0d;
        private TransitiveProps transitiveProps;

        public BBTransitiveAttribute(Block block) : base(block)
        {
            transitiveProps = new TransitiveProps(block);
        }

        /// <summary>
        /// Mostly used for manual initialization but also called when JSON patch applies this behavior.
        /// </summary>
        /// <param name="properties">will only have values when JSON patches apply this behavior</param>
        public override void Initialize(JsonObject properties)
        {
            base.Initialize(properties);

            //OriginsLogger.Debug(api, "[BBTransitiveAttribute] Initializing " + block.Code);

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

            StringBuilder drop_list = new StringBuilder("[");

            foreach (BlockDropItemStack stack in slot.Itemstack.Block.Drops)
            {
                //drop_list.Append(stack.Attributes.Token[attr_list[0]]?.Value<double>());
                drop_list.Append(stack.ResolvedItemstack.Item.Code);
                if (stack.ResolvedItemstack.ItemAttributes.KeyExists(attr_list_name))
                {
                    drop_list.Append("(mutationRate:" + stack.ResolvedItemstack.Attributes.GetDouble("mutationRate") + ")");
                }
                drop_list.Append(", ");
            }
            drop_list.Remove(drop_list.Length - 2, 2);
            drop_list.Append(']');

            OriginsLogger.Debug(byEntity.Api, "  drops: " + slot.Itemstack.GetName() + " is: " + drop_list.ToString());

            base.OnHeldAttackStart(slot, byEntity, blockSel, entitySel, ref handHandling, ref handling);
        }

        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
            dsc.AppendLine("Mutation Rate: " + inSlot.Itemstack.Attributes[attr_list[0]]);
        }

        public override string GetPlacedBlockInfo(IWorldAccessor world, BlockPos pos, IPlayer forPlayer)
        {
            return "Mutation Rate: " + world.BlockAccessor.GetBlock(pos).GetBehavior<BBTransitiveAttribute>()?.mutation ?? "unknown";
        }

        /// <summary>
        /// This runs in BlockCrop as a BlockBehavior when a crop is broken: it is one of the first things to run.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="pos"></param>
        /// <param name="byPlayer"></param>
        /// <param name="handling"></param>
        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, ref EnumHandling handling)
        {
            base.OnBlockBroken(world, pos, byPlayer, ref handling);

            foreach (BlockDropItemStack stack in block.Drops)
            {
                if (stack.ResolvedItemstack.ItemAttributes.KeyExists(attr_list_name))
                {
                    if (stack.ResolvedItemstack.ItemAttributes.KeyExists(attr_list[0]))
                        stack.ResolvedItemstack.Attributes.SetDouble(attr_list[0], Mutate());
                }
            }
        }

        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, ref float dropChanceMultiplier, ref EnumHandling handling)
        {
            List<ItemStack> mutatedSeeds = new List<ItemStack>(1);

            ItemStack seed = new ItemStack(world.GetItem(1709));
            seed.Attributes[attr_list[0]] = new DoubleAttribute(Mutate());

            mutatedSeeds.Add(seed);

            // TODO(chris): set block.Attributes["debuffUnaffectedDrops"] for the modified ItemStacks that *must* drop.
            //     This is so that BlockEntityFarmland knows not to apply random drop numbers to it.
            //     This will have to be found by searching the block below to confirm farmland, then just assigning by hand, I think.
            //     This also must be done somewhere else.

            return mutatedSeeds.ToArray();
        }

        private double Mutate()
        {
            return mutation + normalDistribution(random.NextDouble());
        }

        private delegate double NormalDistribution(double x);

        /// <summary>
        /// Creates a normal distribution centered around mu translated down for a mix of negative numbers and positive numbers.
        /// </summary>
        /// <param name="mu">mean</param>
        /// <param name="sigma">standard deviation</param>
        /// <returns></returns>
        private static NormalDistribution GetNormalDistribution(double mu, double sigma)
        {
            return (double x) =>
            {
                // NOTE(chris): denominator is applied with
                //     numerator * Math.ReciprocalEstimate(denominator)
                //     because we're alredy using the Math library

                // NOTE(chris): I'm sorry! :_(
                double numerator, denominator;

                // numerator
                numerator = Math.Pow(x - mu, 2);
                denominator = 2 * Math.Pow(sigma, 2);

                numerator = Math.Exp(-1 * (numerator * Math.ReciprocalEstimate(denominator)));

                // denominator
                denominator *= Math.PI;

                numerator = numerator * Math.ReciprocalSqrtEstimate(denominator) - 0.95d;
                denominator = 10;
                return numerator * Math.ReciprocalEstimate(denominator);
            };
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
                    behavior.transitiveProps = new TransitiveProps(block);

                    JsonObject properties = new JsonObject(new JObject());

                    behavior.Initialize(properties);

                    // since VSEssentials adds to both, we cannot vary from this practice
                    block.CollectibleBehaviors = block.CollectibleBehaviors.Append(behavior);
                    block.BlockBehaviors = block.BlockBehaviors.Append(behavior);

                    if (block.CropProps != null)
                    {
                        block.CropProps.Behaviors = block.CropProps.Behaviors.Append(behavior.transitiveProps);
                    }
                    else
                    {
                        OriginsLogger.Debug(api, "unable to set block crop behavior!");
                    }
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
