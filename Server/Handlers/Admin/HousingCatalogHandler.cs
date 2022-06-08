using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;

namespace Server.Handlers.Admin;

public class HousingCatalogHandler : ISingletonScript
{
    private readonly HouseService _houseService;

    public HousingCatalogHandler(HouseService houseService)
    {
        _houseService = houseService;

        AltAsync.OnClient<ServerPlayer>("housingcatalog:open", OnOpenHousingCatalog);
        AltAsync.OnClient<ServerPlayer, int>("housingcatalog:requestdetails", OnRequestDetails);
    }

    private async void OnOpenHousingCatalog(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.AccountModel.Permission.HasFlag(Permission.STAFF))
        {
            return;
        }

        player.EmitGui("housingcatalog:setup", await _houseService.GetAll());
    }

    private async void OnRequestDetails(ServerPlayer player, int houseId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.AccountModel.Permission.HasFlag(Permission.STAFF))
        {
            return;
        }

        var house = await _houseService.GetByKey(houseId);

        player.EmitGui("housingcatalog:opendetails", house);
    }
}