using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Modules.Houses;

namespace Server.Handlers.House;

public class SellHouseHandler : ISingletonScript
{
    private readonly HouseModule _houseModule;
    private readonly HouseService _houseService;

    public SellHouseHandler(
        HouseService houseService,
        HouseModule houseModule)
    {
        _houseService = houseService;
        _houseModule = houseModule;

        AltAsync.OnClient<ServerPlayer, int>("housedialog:sell", OnSellHouse);
    }

    private async void OnSellHouse(ServerPlayer player, int bankAccountId)
    {
        if (!player.Exists)
        {
            return;
        }

        var house = await _houseService.GetByDistance(player.Position);
        if (house == null)
        {
            player.SendNotification("Es ist keine Immobilie in der Nähe.", NotificationType.ERROR);
            return;
        }

        await _houseModule.Sell(player, house, bankAccountId);
    }
}