using Newtonsoft.Json.Linq;
using Origins.Systems;
using Origins.Systems.Horticulture;
using System;
using System.Linq;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using static Origins.Systems.Horticulture.HorticultureSystem;

namespace Origins.Patches.Behaviors
{
    public class BlockBehaviorGenetic : BlockBehavior, ICodePatch
    {
        static readonly string attr_list_name = "genetic_attributes";

        static ICoreAPI api;
        static Random random = new Random();
        static double mu = 0.5;
        static double sigma = Math.ReciprocalEstimate(3.92d);
        static NormalDistribution normalDistribution = GetNormalDistribution(mu, sigma);

        public BlockBehaviorGenetic(Block block) : base(block)
        {
        }

        /// <summary>
        /// Mostly used for manual initialization but also called when JSON patch applies this behavior.
        /// </summary>
        /// <param name="properties">will only have values when JSON patches apply this behavior</param>
        public override void Initialize(JsonObject properties)
        {
            base.Initialize(properties);

            //if (!propertiesAtString.Equals("{}"))
            //    api.Logger.Debug("Properties:\n{0}", propertiesAtString);

            // the code until the end of the foreach loop is for making sure collectible objects retain externally defined Attributes
            // ensures (transitive) attribute list is in block's 'Attributes'
            block.Attributes ??= properties ?? new JsonObject(new JObject());
            block.Attributes.Token[attr_list_name] ??= properties.Token[attr_list_name];
        }

        private bool StackHasAttribute(ItemSlot slot, GeneticData attr)
        {
            return slot.Itemstack.Attributes.HasAttribute(attr.name);
        }

        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

            if (inSlot.Itemstack.ItemAttributes.Exists)
            {
                // ItemAttributes to iterate over attributes the ItemStack should have
                // Format: attr_name: attr_val
                foreach (GeneticData attr in inSlot.Itemstack.ItemAttributes[attr_list_name].AsArray<GeneticData>())
                {
                    dsc.Append(attr.name).Append(": ");

                    if (!StackHasAttribute(inSlot, attr))
                    {
                        inSlot.Itemstack.Attributes.SetDouble(attr.name, Math.Round(1.0d, 2));
                        //inSlot.Itemstack.Attributes.SetDouble(attr.name, Math.Round(attr.init, 2));
                    }

                    dsc.AppendLine(inSlot.Itemstack.Attributes.GetAsString(attr.name));
                }
            }
        }

        public override string GetPlacedBlockInfo(IWorldAccessor world, BlockPos pos, IPlayer forPlayer)
        {
            return "Attributes: " + world.Api.ModLoader.GetModSystem<HorticultureSystem>()?.GetAttributes(pos) ?? "unknown";
        }

        public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack, ref EnumHandling handling)
        {
            HorticultureSystem hortsys = world.Api.ModLoader.GetModSystem<HorticultureSystem>();
            if (hortsys == null)
            {
                return false;
            }

            ItemStack stack = byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack;
            var attrs = stack.Attributes.AsQueryable()
                .Where(attr => stack.ItemAttributes[attr_list_name].KeyExists(attr.Key))
                .ToArray();

            var tree = new TreeAttribute();

            foreach (var attr in attrs)
            {
                tree.SetAttribute(attr.Key, attr.Value);
            }

            hortsys.SetAttributes(blockSel.Position, tree);
            //ITreeAttribute foo = hortsys.GetAttributes(blockSel.Position);

            return base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack, ref handling);
        }

        public override void OnBlockPlaced(IWorldAccessor world, BlockPos blockPos, ref EnumHandling handling)
        {
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

            HorticultureSystem hortsys = world.Api.ModLoader.GetModSystem<HorticultureSystem>();
            ITreeAttribute attrs = hortsys?.GetAttributes(pos);

            foreach (BlockDropItemStack stack in block.Drops)
            {
                //if (stack.ResolvedItemstack.ItemAttributes.KeyExists(attr_list_name))
                //{
                //    foreach (var attr in )
                //    {
                //        stack.ResolvedItemstack.Attributes[attr.Key] = new DoubleAttribute(Mutate(attrs.GetDouble(attr.Key)));
                //    }
                //}
            }
        }

        //public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, ref float dropChanceMultiplier, ref EnumHandling handling)
        //{
        //    List<ItemStack> mutatedSeeds = new List<ItemStack>(1);

        //    ItemStack seed = new ItemStack(world.GetItem(1709));
        //    seed.Attributes[attr_list[0]] = new DoubleAttribute(Mutate(1.0d));

        //    mutatedSeeds.Add(seed);

        //    // TODO(chris): set block.Attributes["debuffUnaffectedDrops"] for the modified ItemStacks that *must* drop.
        //    //     This is so that BlockEntityFarmland knows not to apply random drop numbers to it.
        //    //     This will have to be found by searching the block below to confirm farmland, then just assigning by hand, I think.
        //    //     This also must be done somewhere else.

        //    return mutatedSeeds.ToArray();
        //}

        private double Mutate(double mutation)
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

                if (block is BlockCrop)
                {
                    BlockBehaviorGenetic behavior = new BlockBehaviorGenetic(block);

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
            BlockBehaviorGenetic.api = api;

            OriginsLogger.Debug(api, "[BlockBehaviorGenetic] Registering patch: BlockBehaviorGenetic");

            api.RegisterCollectibleBehaviorClass("BlockBehaviorGenetic", typeof(BlockBehaviorGenetic));
            api.RegisterBlockBehaviorClass("BlockBehaviorGenetic", typeof(BlockBehaviorGenetic));
        }
    }
}
