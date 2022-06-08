using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Modules.Houses;

namespace Server.Handlers.House;

public class EnterExitHouseHandler : ISingletonScript
{
    private readonly HouseModule _houseModule;

    public EnterExitHouseHandler(HouseModule houseModule)
    {
        _houseModule = houseModule;

        AltAsync.OnClient<ServerPlayer>("house:enterexit", OnEnterExitHouse);
    }

    private async void OnEnterExitHouse(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        if (player.Dimension == 0)
        {
            await _houseModule.Enter(player);
        }
        else
        {
            await _houseModule.Exit(player);
        }
    }
}