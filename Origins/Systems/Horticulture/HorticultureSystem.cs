using Origins.Util;
using System;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace Origins.GameContent;

internal class HorticultureSystem : ModSystem
{
    public override void StartPre(ICoreAPI api)
    {
    }

    public override void Start(ICoreAPI api)
    {
        api.RegisterItemClass("ItemSeedAnalyzer", typeof(ItemSeedAnalyzer));
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        SeedAnalyzer = new HudSeedAnalyzer(api);
        // TODO (chris): way down the line, "wearable" has related keybind (like glasses)
        /*
        api.Input.RegisterHotKey(
            "Skill Interface",
            "Opens up the Skills GUI",
            GlKeys.O,
            HotkeyType.GUIOrOtherControls);

        api.Input.SetHotKeyHandler("Skill Interface", ToggleGUI);//*/
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        base.StartServerSide(api);

        Player.sapi = api;

        api.Event.PlayerJoin += Player.Event_PlayerJoin;
    }


    HudSeedAnalyzer SeedAnalyzer;

    [Obsolete("This is just on the back burner right now")]
    public HudSeedAnalyzer GetHudSeedAnalyzer() => SeedAnalyzer;
}

/// <summary>
/// This will need to be its own thing down the line.
/// </summary>
internal class Player
{
    static readonly string[] attr_list = new string[] { "mutation" };
    private static Player[] state = new Player[1];
    internal static ICoreServerAPI sapi;

    internal static void Event_PlayerJoin(IServerPlayer byPlayer)
    {
        state = state.Append(new Player(byPlayer));
    }


    private readonly IServerPlayer player;

    internal Player(IServerPlayer player)
    {
        this.player = player;
        this.player.InWorldAction += Player_InWorldAction;
    }

    internal void Player_InWorldAction(EnumEntityAction action, bool on, ref EnumHandling handled)
    {
        switch (action)
        {
            case EnumEntityAction.None:
                break;
            case EnumEntityAction.Forward:
                break;
            case EnumEntityAction.Backward:
                break;
            case EnumEntityAction.Left:
                break;
            case EnumEntityAction.Right:
                break;
            case EnumEntityAction.Jump:
                break;
            case EnumEntityAction.Sneak:
                break;
            case EnumEntityAction.Sprint:
                break;
            case EnumEntityAction.Glide:
                break;
            case EnumEntityAction.FloorSit:
                break;
            case EnumEntityAction.LeftMouseDown:
                break;
            case EnumEntityAction.RightMouseDown:
                OriginsLogger.Debug(player.Entity.Api, "[Player::Player_InWorldAction] Right clicked!");
                CheckSeedPlanting();
                break;
            case EnumEntityAction.Up:
                break;
            case EnumEntityAction.Down:
                break;
            case EnumEntityAction.CtrlKey:
                break;
            case EnumEntityAction.ShiftKey:
                break;
            case EnumEntityAction.InWorldLeftMouseDown:
                break;
            case EnumEntityAction.InWorldRightMouseDown:
                break;
            default:
                break;
        }
    }

    // TODO(chris): refine this, it seems so specific.
    private void CheckSeedPlanting()
    {
        ItemStack activeStack = player.InventoryManager.ActiveHotbarSlot.Itemstack;
        if (activeStack?.Item == null || !activeStack.Item.Code.BeginsWith("game", "seeds"))
        {
            return;
        }


        if (player.CurrentBlockSelection == null || !player.CurrentBlockSelection.Block.Code.PathStartsWith("farmland"))
        {
            return;
        }

        BEBehaviorFarmlandGeneticData bebfarmland = player.CurrentBlockSelection.Block
            .GetBEBehavior<BEBehaviorFarmlandGeneticData>(player.CurrentBlockSelection.Position);

        if (false == ((BlockEntityFarmland)bebfarmland.Blockentity).CanPlant())
        {
            return;
        }

        // NOTE(chris): set genetic data in farmland blockentity
        bebfarmland.Mutation = activeStack.Attributes.GetDouble(attr_list[0]);
        OriginsLogger.Debug(player.Entity.Api,
            "[Player::CheckSeedPlanting] Successfully saved genetic data: {0}",
            bebfarmland.Mutation
        );

        if (activeStack.Attributes.GetTreeAttribute("genes") is TreeAttribute tree)
        // instance values
        {
            bebfarmland.Genes ??= new SyncedTreeAttribute();
            bebfarmland.Genes.MergeTree(tree);
        }
        // default values
        else if (activeStack.ItemAttributes.ToAttribute() is TreeAttribute defaults)
        {
            bebfarmland.Genes ??= new SyncedTreeAttribute();
            bebfarmland.Genes
                .MergeTree(
                    defaults.GetTreeAttribute("genes")
                );
        }
    }
}
