using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;

namespace Server.Handlers.Admin;

public class ItemCatalogHandler : ISingletonScript
{
    private readonly ItemCatalogService _itemCatalogService;

    public ItemCatalogHandler(ItemCatalogService itemCatalogService)
    {
        _itemCatalogService = itemCatalogService;

        AltAsync.OnClient<ServerPlayer>("itemcatalog:open", OnOpenItemCatalog);
    }

    private async void OnOpenItemCatalog(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.AccountModel.Permission.HasFlag(Permission.STAFF))
        {
            return;
        }

        player.EmitGui("itemcatalog:open", await _itemCatalogService.GetAll());
    }
}