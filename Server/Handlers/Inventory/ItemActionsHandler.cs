using System.Collections.Generic;
using System.Linq;
using AltV.Net.Async;
using AltV.Net.Data;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Models;
using Server.Data.Sets;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Inventory;
using Server.Helper;
using Server.Modules.Clothing;
using Server.Modules.Consumeable;

namespace Server.Handlers.Inventory;

public class ItemActionHandler : ISingletonScript
{
    private readonly GroupFactionService _groupFactionService;
    private readonly ItemService _itemService;
    private readonly ItemWeaponAttachmentService _itemWeaponAttachmentService;
    private readonly Serializer _serializer;

    public ItemActionHandler(Serializer serializer, ItemService itemService,
        ItemWeaponAttachmentService itemWeaponAttachmentService, GroupFactionService groupFactionService)
    {
        _serializer = serializer;
        _itemService = itemService;
        _itemWeaponAttachmentService = itemWeaponAttachmentService;
        _groupFactionService = groupFactionService;

        AltAsync.OnClient<ServerPlayer, int>("itemactions:get", OnGetItemActions);
    }

    private async void OnGetItemActions(ServerPlayer player, int itemId)
    {
        if (!player.Exists)
        {
            return;
        }

        var item = await _itemService.GetByKey(itemId);
        if (item == null)
        {
            return;
        }

        var itemActions = ItemActionsSet.Get(item.CatalogItemModelId);

        var actions = new List<ActionData>
        {
            new($"{GetItemName(item)} ablegen", "item:placeonground"),
            new($"{GetItemName(item)} Notiz setzen", "item:setnote"),
            new($"{GetItemName(item)} vergeben", "item:getplayersaround")
        };

        actions.AddRange(itemActions);

        if (ConsumeableItemModule.IsConsumeable(item.CatalogItemModelId))
        {
            actions.Add(new ActionData($"{GetItemName(item)} konsumieren", "item:consume"));
        }

        if (item.CatalogItemModel.Equippable)
        {
            actions.Add(new ActionData("Kleidungsstück umbenennen", "item:rename"));

            switch (item.ItemState)
            {
                case ItemState.NOT_EQUIPPED:
                    actions.Add(new ActionData($"{GetItemName(item)} anziehen", "item:equip"));
                    break;
                case ItemState.EQUIPPED:
                    actions.Add(new ActionData($"{GetItemName(item)} ausziehen", "item:unequip"));
                    break;
            }
        }

        if (item.Amount > 1)
        {
            actions.Add(new ActionData($"{GetItemName(item)} aufteilen", "item:split"));
        }

        if (item is ItemWeaponModel)
        {
            var attachments = await _itemWeaponAttachmentService.Where(i => i.ItemWeaponId == itemId);
            actions.AddRange(attachments.Select(itemWeaponAttachment =>
                new ActionData($"{GetItemName(itemWeaponAttachment)} abmontieren", "item:removeattachment",
                    itemWeaponAttachment.Id.ToString())));
        }

        if (player.IsAduty)
        {
            actions.Add(new ActionData($"[Admin] {GetItemName(item)} löschen", "item:delete"));
        }

        var factionGroup = await _groupFactionService.GetFactionByCharacter(player.CharacterModel.Id);
        if (factionGroup is { FactionType: FactionType.POLICE_DEPARTMENT })
        {
            // Evidence Room Position
            if (player.Position.Distance(new Position(367.13406f, -1606.9055f, 28.279907f)) < 3.0f)
            {
                actions.Add(new ActionData($"{GetItemName(item)} in Asservatenkammer geben", "item:toevidenceroom"));
            }
        }

        // Reset all interactions if the item is not in the players inventory.
        if (item.InventoryModelId != player.CharacterModel.InventoryModel.Id)
        {
            actions = new List<ActionData>();
        }

        player.EmitGui("itemactions:opencontextmenu", actions);
    }

    private string GetItemName(ItemModel itemModel)
    {
        if (ClothingModule.IsClothesOrProp(itemModel.CatalogItemModelId))
        {
            if (string.IsNullOrEmpty(itemModel.CustomData))
            {
                return "";
            }

            var data = _serializer.Deserialize<ClothingData>(itemModel.CustomData);
            return data.Title;
        }

        return itemModel.CatalogItemModel.Name;
    }
}