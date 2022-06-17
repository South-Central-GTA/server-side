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

namespace Server.ChatCommands.Main;

public class UnCuffPlayerCommand : ISingletonScript
{
    private readonly InventoryModule _inventoryModule;
    private readonly ClothingModule _clothingModule;
    private readonly ItemService _itemService;

    public UnCuffPlayerCommand(InventoryModule inventoryModule, ClothingModule clothingModule, ItemService itemService)
    {
        _inventoryModule = inventoryModule;
        _clothingModule = clothingModule;
        _itemService = itemService;
    }

       [Command("uncuff", "Nehmen einen Charakter Handschellen ab wenn du den Schlüssel dafür hast.", Permission.NONE,
        new[] { "Spieler ID" })]
    public async void OnUncuff(ServerPlayer player, string expectedPlayerId)
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

        if (player == target)
        {
            player.SendNotification("Du kannst den Befehl nicht an dir selbst nutzen.", NotificationType.ERROR);
            return;
        }

        if (player.Position.Distance(target.Position) > 2)
        {
            player.SendNotification($"Dein Charakter ist von {target.CharacterModel.Name} zu weit entfernt.",
                NotificationType.ERROR);
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

        if (itemHandCuff.GroupModelId.HasValue)
        {
            if (player.CharacterModel.InventoryModel.Items.Where(i => i.CatalogItemModelId == ItemCatalogIds.GROUP_KEY)
                .Cast<ItemGroupKeyModel>().All(i => i.GroupModelId != itemHandCuff.GroupModelId.Value))
            {
                player.SendNotification(
                    "Dein Charakter hat keinen Gruppenschlüssel von der Gruppe welche die Handschellen angelegt hat im Inventar.",
                    NotificationType.ERROR);
                return;
            }
        }
        else if (itemHandCuff.ItemKeyModelId.HasValue)
        {
            if (player.CharacterModel.InventoryModel.Items.All(i => i.Id != itemHandCuff.ItemKeyModelId.Value))
            {
                player.SendNotification("Dein Charakter hat kein Schlüssel für diese Handschellen.",
                    NotificationType.ERROR);
                return;
            }

            var keyItem = await _itemService.GetByKey(itemHandCuff.ItemKeyModelId.Value);
            await _itemService.Remove(keyItem);
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
            $"Deinem Charakter wurden von {player.CharacterModel.Name} die Handschellen abgenommen.",
            NotificationType.INFO);
        player.SendNotification($"Dein Charakter hat {target.CharacterModel.Name} Handschellen abgenommen.",
            NotificationType.SUCCESS);
    }
}