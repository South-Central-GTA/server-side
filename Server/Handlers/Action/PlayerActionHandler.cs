using System.Collections.Generic;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Data.Models;
using Server.Modules.Context;

namespace Server.Handlers.Action;

public class PlayerActionHandler : ISingletonScript
{
    private readonly ContextModule _contextModule;

    public PlayerActionHandler(ContextModule contextModule)
    {
        _contextModule = contextModule;
        AltAsync.OnClient<ServerPlayer>("playeractions:get", OnGetActions);
    }

    private async void OnGetActions(ServerPlayer player)
    {
        await AltAsync.Do(() =>
        {
            if (!player.Exists)
            {
                return;
            }

            var actions = new List<ActionData> { new("Inventar öffnen", "inventory:request") };

            _contextModule.OpenMenu(player, player.CharacterModel.Name, actions);
        });
    }
}