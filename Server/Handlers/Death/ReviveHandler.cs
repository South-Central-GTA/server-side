using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Modules.Death;

namespace Server.Handlers.Death;

public class ReviveHandler : ISingletonScript
{
    private readonly ReviveModule _reviveModule;

    public ReviveHandler(ReviveModule reviveModule)
    {
        _reviveModule = reviveModule;
        
        AltAsync.OnClient<ServerPlayer, ushort>("revive:reviveplayer", OnPlayerRevive);
    }

    private async void OnPlayerRevive(ServerPlayer player, ushort playerId)
    {
        await _reviveModule.RevivePlayer(player, playerId);
    }
}