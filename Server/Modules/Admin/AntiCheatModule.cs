using System.Linq;
using System.Threading.Tasks;
using AltV.Net.Data;
using Microsoft.Extensions.Logging;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Database.Models.Inventory;

namespace Server.Modules.Admin;

public class AntiCheatModule
    : ITransientScript
{
    private readonly GroupService _groupService;
    private readonly ItemClothService _itemClothService;
    private readonly ILogger<AntiCheatModule> _logger;

    public AntiCheatModule(
        ILogger<AntiCheatModule> logger,
        GroupService groupService,
        ItemClothService itemClothService)
    {
        _logger = logger;
        _groupService = groupService;
        _itemClothService = itemClothService;
    }

    public bool DetectDropItemPositionHack(ServerPlayer player, Position position)
    {
        var valid = !(player.Position.Distance(position) > 3);

        if (!valid)
        {
            _logger.LogWarning("Detect drop item position hack.");
        }

        return valid;
    }

    public async Task<bool> DetectSwitchItemHack(ServerPlayer player, InventoryModel inv, ItemModel itemModel)
    {
        // Check if the inventory is the player inventory.
        if (inv.CharacterModelId == player.CharacterModel.Id)
        {
            return false;
        }

        // Check if the inventory could be the house inventory.
        if (inv.HouseModelId == player.Dimension)
        {
            return false;
        }

        // Check if the inventory could be the trunk.
        if (player.HasData("INTERACT_VEHICLE_TRUNK"))
        {
            player.GetData("INTERACT_VEHICLE_TRUNK", out int vehicleDbId);
            if (inv.VehicleModelId == vehicleDbId)
            {
                return false;
            }
        }

        // Check if the inventory could be the group inventory.
        if (inv.GroupId.HasValue)
        {
            var group = await _groupService.GetByKey(inv.GroupId.Value);
            var member = group.Members.Find(m => m.CharacterModelId == player.CharacterModel.Id);

            if (member == null)
            {
                return true;
            }

            if (inv.GroupCharacterId == player.CharacterModel.Id || member.Owner)
            {
                return false;
            }
        }

        // Check if inventory was a cloth item inventory from the player.
        foreach (var cloth in player.CharacterModel.InventoryModel.Items.Where(i => i is ItemClothModel))
        {
            var itemCloth = await _itemClothService.GetByKey(cloth.Id);
            if (itemCloth.ClothingInventoryModel != null)
            {
                // Check if the id matchs but is not the item itslef.
                if (itemCloth.ClothingInventoryModel.Id == inv.Id && itemCloth.Id != itemModel.Id)
                {
                    return false;
                }
            }
        }

        // Check if the inventory still contians the item.
        if (inv.Items.Contains(itemModel))
        {
            return false;
        }

        return true;
    }

    public bool DetectStackItemHack(ItemModel draggingItemModel, ItemModel droppedItemModel)
    {
        if (draggingItemModel.Id == droppedItemModel.Id)
        {
            return true;
        }

        return false;
    }

    public bool DetectSwapItemHack(ItemModel draggingItemModel, ItemModel droppedItemModel)
    {
        if (draggingItemModel.Id == droppedItemModel.Id)
        {
            return true;
        }

        return false;
    }

    public bool DetectPickupItemHack(ItemModel itemModel)
    {
        if (itemModel.InventoryModelId.HasValue)
        {
            return true;
        }

        return false;
    }
}