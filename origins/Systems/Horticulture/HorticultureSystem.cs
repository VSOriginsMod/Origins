using Origins.Gui;
using System.Collections;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace Origins.Systems.Horticulture
{
    // TODO (chris): add permanence for "Genetics" hashtable
    internal class HorticultureSystem : ModSystem
    {
        /// <summary>
        /// Stores CropBlockProperties by UUID.
        /// </summary>
        Hashtable GeneticsTable;

        HudSeedAnalyzer SeedAnalyzer;

        public override double ExecuteOrder()
        {
            return base.ExecuteOrder();
        }

        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return true;
        }

        public override void StartPre(ICoreAPI api)
        {
            // TODO (chris): load hashtable from permanent (world) data
            // HashProvider IEqualityComparer IComparer
            GeneticsTable = new Hashtable();
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

        public HudSeedAnalyzer GetHudSeedAnalyzer()
        {
            return SeedAnalyzer;
        }
    }
}
