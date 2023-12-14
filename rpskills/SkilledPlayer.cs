using Vintagestory.API.Common;

namespace rpskills
{
    /// <summary>
    /// FIXME(anakin!): I am an artifact ready for the roots functionality
    /// </summary>
    class SkilledPlayer : EntityPlayer
    {
        public SkilledPlayer() : base()
        {
            this.Stats
                .Register("combatant", EnumStatBlendType.FlatSum)
                .Register("farmer ", EnumStatBlendType.FlatSum)
                .Register("homekeeper", EnumStatBlendType.FlatSum)
                .Register("hunter", EnumStatBlendType.FlatSum)
                .Register("miner", EnumStatBlendType.FlatSum)
                .Register("processer", EnumStatBlendType.FlatSum)
                .Register("rancher", EnumStatBlendType.FlatSum)
                .Register("smith", EnumStatBlendType.FlatSum)
                .Register("woodsman", EnumStatBlendType.FlatSum);
        }
    }
}