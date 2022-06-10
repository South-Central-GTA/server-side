using AltV.Net.Async;
using AltV.Net.Data;
using Microsoft.Extensions.Logging;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;

namespace Server.Modules.Admin;

public class FreecamModule : ITransientScript
{
    private readonly ILogger<FreecamModule> _logger;

    public FreecamModule(ILogger<FreecamModule> logger)
    {
        _logger = logger;
    }

    public void Start(ServerPlayer player, ServerPlayer target = null!)
    {
        lock (player)
        {
            player.SetStreamSyncedMetaData("FREECAM", true);
            player.EmitLocked("freecam:open");
        }
    }

    public void Stop(ServerPlayer player, bool teleportToPosition = true)
    {
        lock (player)
        {
            player.DeleteStreamSyncedMetaData("FREECAM");
            player.EmitLocked("freecam:close", teleportToPosition);
        }
    }

    public void SetPosition(ServerPlayer player, Position position)
    {
        player.EmitLocked("freecam:setpos", position);
    }
}