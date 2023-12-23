

namespace rpskills.CoreSys
{
    
    class HeritageSelectedState
    {
        public bool DidSelect;
    }

    /// <summary>
    /// Contains all data regarding Heritage selection.
    /// 
    /// See Vintagestory.GameContent.CharacterSelectionPacket for more details.
    /// </summary>
    // TODO(chris): What is this? I found it on CharacterSelectionPacket, and
    //              a constructor was implicitly defined. For now, I'll
    //              explicitly define.
    // [ProtoContract(ImplicitFields = 1)]
    public class HeritageSelectionPacket
    {
        public HeritageSelectionPacket(bool didSelect, string heritage) {
            this.DidSelect = didSelect;
            this.HeritageName = heritage;
        }

        public bool DidSelect;
        public string HeritageName { get; private set;}
    }
}