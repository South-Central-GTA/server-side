using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;

namespace Server.Handlers.Interior;

public class InteriorHandler : ISingletonScript
{
    public InteriorHandler()
    {
        AltAsync.OnClient<ServerPlayer, uint>("interior:enter", OnEnteredInterior);
        AltAsync.OnClient<ServerPlayer, uint>("interior:left", OnLeavedInterior);
    }

    private async void OnEnteredInterior(ServerPlayer player, uint mloInterior)
    {
        if (!player.Exists)
        {
            return;
        }

        player.MloInterior = mloInterior;
    }

    private async void OnLeavedInterior(ServerPlayer player, uint mloInterior)
    {
        if (!player.Exists)
        {
            return;
        }

        player.MloInterior = mloInterior;
    }
}