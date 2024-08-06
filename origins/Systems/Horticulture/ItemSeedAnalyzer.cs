using Origins.Config;
using Origins.Gui;
using ProtoBuf.Meta;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.Client.NoObf;

namespace Origins.Systems.Horticulture
{
    internal class ItemSeedAnalyzer : Item
    {
        bool hortsysEnabled = false;
        HorticultureSystem HortSys;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            hortsysEnabled = api.ModLoader.IsModSystemEnabled("Origins.Systems.Horticulture.HorticultureSystem");
            
        }

        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            handling = EnumHandHandling.NotHandled;

            // TODO (chris): CONFIG OPTION; add setting to change between "Toggle" and "Continuous" behavior
            if (!HortSysLoaded())
            {
                api.Logger.Error("Oops, HorticultureSystem was not found! You should not see this.");
                return;
            }

            HortSys
                .GetHudSeedAnalyzer()
                .WithBlockAndEntitySelection(blockSel, entitySel)
                .Toggle();
        }

        private bool HortSysLoaded()
        {
            if (hortsysEnabled && HortSys == null)
                HortSys = api.ModLoader.GetModSystem<HorticultureSystem>();

            return HortSys != null;
        }
    }
}
