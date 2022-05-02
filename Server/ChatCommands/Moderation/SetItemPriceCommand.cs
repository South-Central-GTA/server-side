using System;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.CommandSystem;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;

namespace Server.ChatCommands.Moderation;

public class SetItemPriceCommand : ISingletonScript
{
    private readonly ItemCatalogService _itemCatalogService;
    
    public SetItemPriceCommand(ItemCatalogService itemCatalogService)
    {
        _itemCatalogService = itemCatalogService;
    }
    
    [Command("setitemprice", "Setze den Preis eines bestimmten Items.", Permission.ECONOMY_MANAGEMENT, new[] { "Item ID", "Preis (Dollar)" })]
    public async void OnExecute(ServerPlayer player, string expectedItemId, string expectedPrice)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!Enum.TryParse<ItemCatalogIds>(expectedItemId, out var itemId))
        {
            player.SendNotification("Bitte gebe eine gültige Item ID an.", NotificationType.ERROR);
            return;
        }

        if (!int.TryParse(expectedPrice, out var price))
        {
            player.SendNotification("Bitte gebe einen gültigen Wert als Preis an.", NotificationType.ERROR);
            return;
        }

        var catalogItem = await _itemCatalogService.GetByKey(itemId);
        if (catalogItem == null)
        {
            player.SendNotification("Es konnte kein Item unter diesen Namen gefunden werden.", NotificationType.ERROR);
            return;
        }

        catalogItem.Price = price;

        await _itemCatalogService.Update(catalogItem);
    }
}