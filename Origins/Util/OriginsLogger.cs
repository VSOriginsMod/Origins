using Origins.GameContent;
using System;
using Vintagestory.API.Common;

//TODO(chris): refactor naming scheme: "Heratige" => "Origins"
//NOTE(chris): all current WARN(chris) in this file indicates client-server
//              interactions. They depend on the network channel feature, which
//              must be created and debugged first.

namespace Origins.Util;

class OriginsLogger
{
    private const string Identifier = "[" + OriginsCoreSystem.Domain + "] ";

    internal static void Debug(ICoreAPI api, string message)
    {
        api.Logger.Debug(Identifier + message);
    }

    internal static void Debug(ICoreAPI api, string format, params object[] args)
    {
        api.Logger.Debug(Identifier + format, args);
    }

    internal static void Error(ICoreAPI api, Exception err)
    {
        api.Logger.Error(err);
    }

    internal static void Error(ICoreAPI api, string format, params object[] args)
    {
        api.Logger.Error(Identifier + format, args);
    }
}
