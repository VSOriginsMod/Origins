using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace origins
{
    /// <summary>
    /// Experimental abstraction for loading defined types from the
    /// file system to static memory. The entry point is Load.
    /// </summary>
    /// <typeparam name="T">Metric as defined in ./origins/assets/config</typeparam>
    internal abstract class ProgressionSystem<T> where T : INamedProgression
    {
        /*
         * aspdfjkladsjfiadspioafipopapioarsiourapo
         * 
         * why did I wirte static?!?!?!?!?!?
         */
        internal static bool Loaded = false;
        internal static List<T> Elements;
        internal static Dictionary<string, T> ElementsByName;

        /// <summary>
        /// Uses `api` to load the contents of `asset_path` into inherited
        /// static space.
        /// </summary>
        /// <param name="api"></param>
        /// <param name="path"></param>
        protected static void Load(ICoreAPI api, string asset_path)
        {
            if (Loaded) return;
            Loaded = true;

            Elements = api.Assets.Get(asset_path).ToObject<List<T>>(null);
            ElementsByName = new Dictionary<string, T>();

            foreach (T element in Elements)
            {
                api.Logger.Debug("Discovered element " + element.Name);
                ElementsByName[element.Name] = element;
            }
        }


        protected ICoreAPI api;

        internal ProgressionSystem(ICoreAPI api)
        {
            this.api = api;

            switch (api.Side)
            {
                case EnumAppSide.Server:
                    ServerInit(api as ICoreServerAPI);
                    break;
                case EnumAppSide.Client:
                    ClientInit(api as ICoreClientAPI);
                    break;
                default:
                    break;
            }
        }

        internal abstract void ClientInit(ICoreClientAPI capi);

        internal abstract void ServerInit(ICoreServerAPI sapi);
    }
}
