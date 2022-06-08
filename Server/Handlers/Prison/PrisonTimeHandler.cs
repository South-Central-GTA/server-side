using System;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Modules.Prison;

namespace Server.Handlers.Prison;

public class PrisonTimeHandler : ISingletonScript
{
    private readonly PrisonModule _prisonModule;

    public PrisonTimeHandler(PrisonModule prisonModule)
    {
        _prisonModule = prisonModule;

        AltAsync.OnClient<ServerPlayer>("prison:checktime", OnExecute);
    }

    private async void OnExecute(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.CharacterModel.JailedUntil.HasValue)
        {
            return;
        }

        if (player.CharacterModel.JailedUntil.Value < DateTime.Now)
        {
            await _prisonModule.ClearPlayerFromPrison(player);
        }
    }
}