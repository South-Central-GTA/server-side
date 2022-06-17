using System.Linq;
using AltV.Net;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.CommandSystem;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Inventory;
using Server.Modules.Clothing;
using Server.Modules.Inventory;

namespace Server.ChatCommands.Moderation;

public class AUnCuffPlayerCommand : ISingletonScript
{
    private readonly InventoryModule _inventoryModule;
    private readonly ClothingModule _clothingModule;
    private readonly ItemService _itemService;

    public AUnCuffPlayerCommand(InventoryModule inventoryModule, ClothingModule clothingModule, ItemService itemService)
    {
        _inventoryModule = inventoryModule;
        _clothingModule = clothingModule;
        _itemService = itemService;
    }

    [Command("auncuff", "Nehmen einen Charakter administrativ Handschellen ab.", Permission.STAFF,
        new[] { "Spieler ID" })]
    public async void OnAUncuff(ServerPlayer player, string expectedPlayerId)
    {
        if (!player.Exists)
        {
            return;
        }

        var target = Alt.GetAllPlayers().GetPlayerById(player, expectedPlayerId);
        if (target == null)
        {
            return;
        }

        var itemHandCuff = (ItemHandCuffModel)target.CharacterModel.InventoryModel.Items.FirstOrDefault(i =>
            i.CatalogItemModelId == ItemCatalogIds.HANDCUFF && i.ItemState == ItemState.FORCE_EQUIPPED);
        if (itemHandCuff == null)
        {
            player.SendNotification("Der Charakter hat keine Handschellen angelegt.", NotificationType.ERROR);
            return;
        }

        if (!await _inventoryModule.CanCarry(player, ItemCatalogIds.HANDCUFF))
        {
            return;
        }

        itemHandCuff.Slot = await _inventoryModule.GetFreeNextSlot(player.CharacterModel.InventoryModel.Id);
        itemHandCuff.InventoryModelId = player.CharacterModel.InventoryModel.Id;
        itemHandCuff.ItemState = ItemState.NOT_EQUIPPED;

        await _itemService.Update(itemHandCuff);

        target.Cuffed = false;
        _clothingModule.UpdateClothes(player);

        await _inventoryModule.UpdateInventoryUiAsync(player);
        await _inventoryModule.UpdateInventoryUiAsync(target);

        target.SendNotification(
            $"Deinem Charakter wurden von {player.AccountName} administartiv die Handschellen abgenommen.",
            NotificationType.INFO);
        player.SendNotification(
            $"Du hast Charakter {target.CharacterModel.Name} administrativ Handschellen abgenommen.",
            NotificationType.SUCCESS);
    }
}