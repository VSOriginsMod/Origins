using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace rpskills
{
    /// <summary>
    /// ModSystem is the base for any VintageStory code mods
    /// </summary>
    public class RPSkillsMod : ModSystem
    {
        /// <summary>
        /// Utility for accessing the server's implemented functionality.
        /// 
        /// "Object Reference to the ICoreServerAPI Interface"
        /// </summary>
        ICoreServerAPI sapi;

        /// <summary>
        /// Server initialization
        /// </summary>
        /// <param name="api">Utility for accessing the server's functionality</param>
        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);
            this.sapi = api;


        }

    }
}
