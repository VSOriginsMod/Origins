using Origins.Systems;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Origins.Patches
{
    public class PatchDebugger
    {
        public static void PrintDebug(ICoreAPI api, JsonObject attributes)
        {
            if (attributes == null || !attributes.KeyExists("transitiveAttributes"))
            {
                string foo = (attributes == null) ? "attributes is null " : "attributes is nonnull ";
                foo += (!attributes.KeyExists("transitiveAttributes")) ? "transitiveAttributes key does not exist" : "transitiveAttributesKey exists";
                OriginsLogger.Debug(api, "ERROR: " + foo);
                return;
            }

            foreach (JsonObject attrObj in attributes["transitiveAttributes"].AsArray())
            {
                string attr = attrObj.AsString("");
                if (string.IsNullOrEmpty(attr))
                {
                    OriginsLogger.Debug(api, "ERROR: while parsing elements of array value of key 'transitiveAttributes'; element is empty");
                    continue;
                }

                OriginsLogger.Debug(api, "  key: " + attr);
                if (attributes.KeyExists(attr))
                {
                    OriginsLogger.Debug(api, "  val: " + attributes[attr]);
                }
                else
                {
                    OriginsLogger.Debug(api, " val: DNE");
                }
            }
        }

        public static void PrintDebug(ICoreAPI api, ITreeAttribute attributes)
        {
            if (attributes == null || !attributes.HasAttribute("transitiveAttributes"))
            {
                string foo = (attributes == null) ? "attributes is null " : "attributes is nonnull ";
                foo += (!attributes.HasAttribute("transitiveAttributes")) ? "transitiveAttributes key does not exist" : "transitiveAttributesKey exists";
                OriginsLogger.Debug(api, "ERROR: " + foo);
                return;
            }

            //OriginsLogger.Debug(api, attributes["transitiveAttributes"].ToJsonToken());
            if (attributes["transitiveAttributes"].GetType() == typeof(StringArrayAttribute))
            {
                StringArrayAttribute arr = (StringArrayAttribute)attributes["transitiveAttributes"].GetValue();
                foreach (string attrKey in arr.value)
                {
                    OriginsLogger.Debug(api, "  key: " + attrKey);
                    if (attributes[attrKey].GetValue() is DoubleAttribute attr)
                    {
                        OriginsLogger.Debug(api, "  val: " + attr.value);
                    }
                    else
                    {
                        OriginsLogger.Debug(api, "  val: NotFloat");
                    }
                }

            }
        }
    }
    /*
    public class BlockBehaviorTransitiveAttributes : BlockBehavior
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
