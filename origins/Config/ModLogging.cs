using Vintagestory.API.Common;

namespace Origins.Config
{
    internal class ModLogging
    {
        private const string Identifier = "[" + ModConstants.Domain + "] ";

        public static void Debug(ICoreAPI api, string message)
        {
            api.Logger.Debug(Identifier + message);
        }
    }
}
