using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace Origins.Patches
{
    public class TransitiveAttributes : IPatch
    {
        static readonly string[] CollectibleClasses =
        {
            "ItemPlantableSeed",
        };

        static readonly string[] BlockClasses =
        {
            "BlockCrop",
        };

        public static void RegisterPatch(ICoreAPI api)
        {
            api.RegisterCollectibleBehaviorClass("CBTransitiveAttributes", typeof(CBTransitiveAttributes));
            api.RegisterCollectibleBehaviorClass("BBTransitiveAttributes", typeof(BBTransitiveAttributes));
            api.RegisterBlockBehaviorClass("BBTransitiveAttributes", typeof(BBTransitiveAttributes));
        }

        public static void ApplyPatch(ICoreAPI api)
        {
            if (api.Side != EnumAppSide.Server)
            {
                return;
            }

            ApplyCollectibleBehavior(api.World.Items);
            ApplyBlockBehavior(api.World.Blocks);
        }

        private static int ApplyCollectibleBehavior(IList<Item> items)
        {
            foreach (Item item in items)
            {
                // first two are necessary to make sure it exists, third is for a robust method of filtering
                if (item == null || item.Code == null || item.Class == null)
                {
                    continue;
                }

                //if (block.Code.PathStartsWith("seeds"))
                if (CollectibleClasses.Contains(item.Class))
                {
                    CBTransitiveAttributes behavior = new CBTransitiveAttributes(item);

                    JsonObject properties = new JsonObject(new JObject());

                    properties.Token["transitiveProperties"] = JToken.FromObject(new string[] { "yeild", "mutationRate" });

                    behavior.Initialize(properties);

                    item.CollectibleBehaviors = item.CollectibleBehaviors.Append(behavior);
                }
            }

            return 0;
        }

        private static int ApplyBlockBehavior(IList<Block> blocks)
        {
            foreach (Block block in blocks)
            {
                // first two are necessary to make sure it exists, third is for a robust method of filtering
                if (block == null || block.Code == null || block.Class == null)
                {
                    continue;
                }

                //if (block.Code.PathStartsWith("crop"))
                if (BlockClasses.Contains(block.Class))
                {
                    BBTransitiveAttributes behavior = new BBTransitiveAttributes(block);

                    // since VSEssentials adds to both, we cannot vary from this practice
                    block.CollectibleBehaviors = block.CollectibleBehaviors.Append(behavior);
                    block.BlockBehaviors = block.BlockBehaviors.Append(behavior);
                }
            }

            return 0;
        }
    }

    public class CBTransitiveAttributes : CollectibleBehavior
    {

        public CBTransitiveAttributes(CollectibleObject collObj) : base(collObj)
        {

        }

        public override void Initialize(JsonObject properties)
        {
            base.Initialize(properties);

            collObj.Attributes ??= new JsonObject(new JObject());

            collObj.Attributes.Token["transitiveProperties"] = properties.Token["transitiveProperties"];

            // these properties will mutate on a normal distribution
            collObj.Attributes.Token["yeild"] ??= JToken.FromObject(0.0f);
            collObj.Attributes.Token["mutationRate"] ??= JToken.FromObject(0.0f);
        }

        public override void OnHeldAttackStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandHandling handHandling, ref EnumHandling handling)
        {
            byEntity.Api.Logger.Debug("Attacking using an item with transitive properties!");
            byEntity.Api.Logger.Debug(slot.GetStackName() + " has transitive properties: ");

            foreach (string key in slot.Itemstack.ItemAttributes.Token["transitiveProperties"].ToArray())
            {
                byEntity.Api.Logger.Debug("   " + key + ": " + slot.Itemstack.ItemAttributes[key]);
            }

            base.OnHeldAttackStart(slot, byEntity, blockSel, entitySel, ref handHandling, ref handling);
        }

        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
        {
            //ItemPlantableSeed
        }
    }

    public class BBTransitiveAttributes : BlockBehavior
    {
        // NOTE: __BLOCKCROP__
        public BBTransitiveAttributes(Block block) : base(block)
        {

        }

        public override void OnBlockPlaced(IWorldAccessor world, BlockPos blockPos, ref EnumHandling handling)
        {
        }

        public override void OnHeldAttackStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandHandling handHandling, ref EnumHandling handling)
        {
            base.OnHeldAttackStart(slot, byEntity, blockSel, entitySel, ref handHandling, ref handling);
            byEntity.Api.Logger.Debug("Attacking using a block with transitive properties!");
        }

        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, ref EnumHandling handling)
        {
            base.OnBlockBroken(world, pos, byPlayer, ref handling);
            world.Api.Logger.Debug(byPlayer.PlayerName + " broke a block with transitive properties: " + world.BlockAccessor.GetBlock(pos).Code.ToString());
        }
    }

    /*
    public class BlockBehaviorTransitiveAttributes : BlockBehavior, IPatch
    {
      public BlockBehaviorTransitiveAttributes(Block block) : base(block)
      {
      }

      public override void Initialize(JsonObject properties)
      {
        base.Initialize(properties);
      }

      public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, ref EnumHandling handling)
      {
        base.OnBlockBroken(world, pos, byPlayer, ref handling);
        if (world.Api.Side == EnumAppSide.Client)
        {
          return;
        }
        var new_val = SkillSystem.IncrementSkill(byPlayer, "horticulture");
        if (new_val != 0)
        {
          world.Api.Logger.Debug("[origins] BlockBehaviorTransitiveAttributes.OnBlockBroken called for " + byPlayer.PlayerName + "! Now at " + new_val);
        }
      }

      public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, ref float dropChanceMultiplier, ref EnumHandling handling)
      {
        world.Api.Logger.Debug("[origins] BlockBehaviorTransitiveAttributes.GetDrops called for " + pos.ToString());
        dropChanceMultiplier = 3;
        BlockEntityFarmland blockEntityFarmland = world.BlockAccessor.GetBlockEntity(pos.DownCopy()) as BlockEntityFarmland;
        if (blockEntityFarmland == null)
        {
          dropChanceMultiplier *= byPlayer?.Entity.Stats.GetBlended("wildCropDropRate") ?? 1f;
        }
        block.SplitDropStacks = true;
        ItemStack[] array = base.GetDrops(world, pos, byPlayer, ref dropChanceMultiplier, ref handling);
        if (blockEntityFarmland == null)
        {
          List<ItemStack> list = new List<ItemStack>();
          ItemStack[] array2 = array;
          foreach (ItemStack itemStack in array2)
          {
            if (!(itemStack.Item is ItemPlantableSeed))
            {
              itemStack.StackSize = GameMath.RoundRandom(world.Rand, BlockCrop.WildCropDropMul * (float)itemStack.StackSize);
            }
            if (itemStack.StackSize > 0)
            {
              list.Add(itemStack);
            }
          }
          array = list.ToArray();
        }
        if (blockEntityFarmland != null)
        {
          array = blockEntityFarmland.GetDrops(array);
        }
        return array;
      }

      public static void RegisterPatch(ICoreAPI api)
      {
        api.RegisterBlockBehaviorClass("GeneticItem", typeof(BlockBehaviorTransitiveAttributes));
      }
    }

    /**/
}
