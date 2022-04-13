using System.Threading.Tasks;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Modules.Death;

namespace Server.Handlers.Death;

public class DeathHandler : ISingletonScript
{
    private readonly DeathModule _deathModule;

    public DeathHandler(DeathModule deathModule)
    {
        _deathModule = deathModule;
        
        AltAsync.OnPlayerDead += (player, killer, weapon) => OnPlayerDead(player as ServerPlayer, killer, weapon);
    }

    private async Task OnPlayerDead(ServerPlayer player, IEntity killer, uint weapon)
    {
        await _deathModule.PreparePlayerDead(player, killer, weapon);
    }
}