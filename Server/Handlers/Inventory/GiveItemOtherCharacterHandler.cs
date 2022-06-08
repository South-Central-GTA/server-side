using System.Linq;
using AltV.Net;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Helper;
using Server.Modules.Clothing;
using Server.Modules.Inventory;

namespace Server.Handlers.Inventory;

public class GiveItemOtherCharacterHandler : ISingletonScript
{
    private readonly InventoryModule _inventoryModule;
    private readonly ItemCreationModule _itemCreationModule;

    private readonly ItemService _itemService;
    private readonly Serializer _serializer;

    public GiveItemOtherCharacterHandler(
        Serializer serializer,
        ItemService itemService,
        InventoryModule inventoryModule,
        ItemCreationModule itemCreationModule)
    {
        _serializer = serializer;

        _itemService = itemService;

        _inventoryModule = inventoryModule;
        _itemCreationModule = itemCreationModule;

        AltAsync.OnClient<ServerPlayer>("item:getplayersaround", OnGetPlayersAround);
        AltAsync.OnClient<ServerPlayer, int, int>("item:giveitemtocharacter", OnGiveItemToCharacter);
    }

    private async void OnGetPlayersAround(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        await AltAsync.Do(() =>
        {
            var nearPlayers = player.GetPlayersAround(1.5f);
            var characters = nearPlayers.Select(p => p.CharacterModel).ToList();

            if (characters.Count == 0)
            {
                player.SendNotification("Es ist Niemand in der Nähe deines Charakters.", NotificationType.ERROR);
                return;
            }

            player.EmitGui("inventory:sendcharactersinrange", characters);
        });
    }

    private async void OnGiveItemToCharacter(ServerPlayer player, int characterId, int itemId)
    {
        if (!player.Exists)
        {
            return;
        }

        var item = await _itemService.GetByKey(itemId);
        if (!item.IsBought)
        {
            player.SendNotification("Dies würde dein Charakter nicht tun.", NotificationType.ERROR);
            return;
        }

        var target = Alt.GetAllPlayers().FindPlayerByCharacterId(characterId);
        if (target == null)
        {
            player.SendNotification("Der Charakter konnte nicht gefunden werden.", NotificationType.ERROR);
            return;
        }

        if (player.Position.Distance(target.Position) > 3)
        {
            player.SendNotification("Dein Charakter ist zu weit weg von dem anderen Charakter.",
                                    NotificationType.ERROR);
            return;
        }

        var canCarry = await _inventoryModule.CanCarry(target, item.CatalogItemModel.Id, item.Amount);
        if (!canCarry)
        {
            player.SendNotification("Der Charakter hat nicht genug Platz im Inventar.",
                                    NotificationType.ERROR);
            return;
        }

        var freeSlot =
            await _inventoryModule.GetFreeNextSlot(target.CharacterModel.InventoryModel.Id,
                                                   item.CatalogItemModel.Weight);
        if (freeSlot == null)
        {
            return;
        }

        await _itemCreationModule.HandleRemoveSpecialItems(player, item);

        var oldState = item.ItemState;
        item.ItemState = ItemState.NOT_EQUIPPED;
        item.Slot = freeSlot.Value;
        item.InventoryModelId = target.CharacterModel.InventoryModel.Id;
        await _itemService.Update(item);

        if (ClothingModule.IsClothesOrProp(item.CatalogItemModelId))
        {
            var data = _serializer.Deserialize<ClothingData>(item.CustomData);
            player.SendNotification(
                $"Dein Charakter hat {target.CharacterModel.Name} das Kleidungsstück {data.Title} gegeben.",
                NotificationType.SUCCESS);
            target.SendNotification(
                $"Dein Charakter hat von {player.CharacterModel.Name} das Kleidungsstück {data.Title} erhalten.",
                NotificationType.INFO);
        }
        else
        {
            player.SendNotification(
                $"Dein Charakter hat {target.CharacterModel.Name} {item.Amount}x {item.CatalogItemModel.Name} gegeben.",
                NotificationType.SUCCESS);
            target.SendNotification(
                $"Dein Charakter hat von {player.CharacterModel.Name} {item.Amount}x {item.CatalogItemModel.Name} erhalten.",
                NotificationType.INFO);
        }

        await _itemCreationModule.HandleGiveSpecialItems(target, item);
        await _inventoryModule.UpdateInventoryUiAsync(target);
        await _inventoryModule.UpdateInventoryUiAsync(player);

        if (oldState == ItemState.EQUIPPED)
        {
            player.UpdateClothes();
        }
    }
}