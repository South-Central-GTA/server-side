using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using Microsoft.Extensions.Logging;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Inventory;
using Server.Helper;

namespace Server.Modules.Inventory;

public class InventoryModule : ITransientScript
{
    private readonly GroupService _groupService;
    private readonly HouseService _houseService;
    private readonly InventoryService _inventoryService;

    private readonly ItemCatalogService _itemCatalogService;
    private readonly ItemClothService _itemClothService;
    private readonly ItemService _itemService;
    private readonly ILogger<InventoryModule> _logger;
    private readonly Serializer _serializer;
    private readonly VehicleService _vehicleService;

    public InventoryModule(ILogger<InventoryModule> logger, Serializer serializer,
        ItemCatalogService itemCatalogService, InventoryService inventoryService, ItemService itemService,
        HouseService houseService, VehicleService vehicleService, GroupService groupService,
        ItemClothService itemClothService)
    {
        _logger = logger;
        _serializer = serializer;

        _itemCatalogService = itemCatalogService;
        _inventoryService = inventoryService;
        _itemService = itemService;
        _houseService = houseService;
        _vehicleService = vehicleService;
        _groupService = groupService;
        _itemClothService = itemClothService;
    }

    public async Task<bool> CanCarry(ServerPlayer player, ItemCatalogIds itemCatalogId, int amount = 1)
    {
        var result = await CanCarry(player.CharacterModel.InventoryModel.Id, itemCatalogId, amount);
        switch (result)
        {
            case CanCarryErrorType.SUCCESS:
                return true;
            case CanCarryErrorType.LIMIT:
                var catalogItem = await _itemCatalogService.GetByKey(itemCatalogId);
                player.SendNotification(
                    $"In einem Inventar können maximal {catalogItem.MaxLimit} Stück des Items '{catalogItem.Name}' liegen.",
                    NotificationType.ERROR);
                return false;
            case CanCarryErrorType.NO_SPACE:
                player.SendNotification("Das Inventar deines Charakters ist voll.", NotificationType.ERROR);
                return false;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public async Task<CanCarryErrorType> CanCarry(int inventoryId, ItemCatalogIds itemCatalogId, int amount = 1)
    {
        var inventory = await _inventoryService.GetByKey(inventoryId);
        var catalogItem = await _itemCatalogService.GetByKey(itemCatalogId);
        var currentInventoryWeight = CurrentWeight(inventory);
        var amountOfItem = inventory.Items.Count(i => i.CatalogItemModelId == itemCatalogId) + amount;

        if (amountOfItem > catalogItem.MaxLimit)
        {
            return CanCarryErrorType.LIMIT;
        }

        var canCarry = currentInventoryWeight + catalogItem.Weight * amount <= inventory.MaxWeight;
        return !canCarry ? CanCarryErrorType.NO_SPACE : CanCarryErrorType.SUCCESS;
    }

    public async Task<bool> CanCarry(ServerPlayer player, int inventoryId, float weight)
    {
        var inventory = await _inventoryService.GetByKey(inventoryId);
        var currentInventoryWeight = CurrentWeight(inventory);
        var canCarry = currentInventoryWeight + weight <= inventory.MaxWeight;
        if (!canCarry)
        {
            player.SendNotification(
                "Das Inventar deines Charakters ist voll, die Items konnten nicht hinzugefügt werden.",
                NotificationType.ERROR);
        }

        return canCarry;
    }

    public async Task<int?> GetFreeNextSlot(int inventoryId, float weight)
    {
        var inventory = await _inventoryService.GetByKey(inventoryId);
        var currentInventoryWeight = CurrentWeight(inventory);
        if (currentInventoryWeight <= inventory.MaxWeight + weight)
        {
            var slots = inventory.Items.Select(i => i.Slot ?? -1);
            var firstAvailable = Enumerable.Range(0, int.MaxValue).Except(slots).First();
            return firstAvailable;
        }

        return null;
    }

    public async Task<int> GetFreeNextSlot(int inventoryId)
    {
        var inventory = await _inventoryService.GetByKey(inventoryId);
        var slots = inventory.Items.Select(i => i.Slot ?? -1);
        var firstAvailable = Enumerable.Range(0, int.MaxValue).Except(slots).First();
        return firstAvailable;
    }

    public int? GetFreeNextSlot(InventoryModel inventoryModel, float weight)
    {
        var currentInventoryWeight = CurrentWeight(inventoryModel);
        if (currentInventoryWeight + weight <= inventoryModel.MaxWeight)
        {
            var slots = inventoryModel.Items.Select(i => i.Slot ?? -1);
            var firstAvailable = Enumerable.Range(0, int.MaxValue).Except(slots).First();
            return firstAvailable;
        }

        return null;
    }

    public float CurrentWeight(InventoryModel inventoryModel)
    {
        return inventoryModel.Items.Sum(i => i.CatalogItemModel.Weight * i.Amount);
    }

    public int? AmountOfItem(InventoryModel inventoryModel, ItemCatalogIds itemCatalogIds)
    {
        var requestedItems = inventoryModel.Items.Where(i => i.CatalogItemModelId == itemCatalogIds).ToList();
        if (requestedItems.Count == 0)
        {
            return null;
        }

        return requestedItems.Sum(i => i.Amount);
    }

    public async Task OpenInventoryUiAsync(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        if (player.CharacterModel.DeathState == DeathState.DEAD)
        {
            player.SendNotification("Du kannst jetzt nicht in das Inventar deines Charakters schauen.",
                NotificationType.ERROR);
            return;
        }

        var inventories = await GetInventories(player);

        player.EmitLocked("inventory:open", inventories);
        player.UpdateMoneyUi();
    }
 
    public void CloseInventory(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        player.OpenInventories = new List<OpenInventoryData>();

        player.EmitLocked("inventory:close");
    }

    public async Task UpdateInventoryUiAsync(ServerPlayer player)
    {
        var inventories = await GetInventories(player);

        player.EmitLocked("inventory:update", inventories);
        player.UpdateMoneyUi();
    }

    public async Task UpdateAmmo(ServerPlayer player, string ammoJson)
    {
        if (!player.Exists)
        {
            return;
        }

        var ammoData = _serializer.Deserialize<AmmoData>(ammoJson);

        var pistolAmmo =
            player.CharacterModel.InventoryModel.Items.FirstOrDefault(i =>
                i.CatalogItemModelId == ItemCatalogIds.AMMO_PISTOL);
        if (pistolAmmo != null)
        {
            pistolAmmo.Amount = ammoData.PistolAmmo;
            if (pistolAmmo.Amount <= 0)
            {
                await _itemService.Remove(pistolAmmo);
            }
            else
            {
                await _itemService.Update(pistolAmmo);
            }
        }

        var machineGunAmmo =
            player.CharacterModel.InventoryModel.Items.FirstOrDefault(i =>
                i.CatalogItemModelId == ItemCatalogIds.AMMO_MACHINE_GUN);
        if (machineGunAmmo != null)
        {
            machineGunAmmo.Amount = ammoData.MachineGunAmmo;
            if (machineGunAmmo.Amount <= 0)
            {
                await _itemService.Remove(machineGunAmmo);
            }
            else
            {
                await _itemService.Update(machineGunAmmo);
            }
        }

        var assaultAmmo =
            player.CharacterModel.InventoryModel.Items.FirstOrDefault(i =>
                i.CatalogItemModelId == ItemCatalogIds.AMMO_ASSAULT);
        if (assaultAmmo != null)
        {
            assaultAmmo.Amount = ammoData.AssaultAmmo;
            if (assaultAmmo.Amount <= 0)
            {
                await _itemService.Remove(assaultAmmo);
            }
            else
            {
                await _itemService.Update(assaultAmmo);
            }
        }

        var sniperAmmo =
            player.CharacterModel.InventoryModel.Items.FirstOrDefault(i =>
                i.CatalogItemModelId == ItemCatalogIds.AMMO_SNIPER);
        if (sniperAmmo != null)
        {
            sniperAmmo.Amount = ammoData.SniperAmmo;
            if (sniperAmmo.Amount <= 0)
            {
                await _itemService.Remove(sniperAmmo);
            }
            else
            {
                await _itemService.Update(sniperAmmo);
            }
        }

        var shotgunAmo =
            player.CharacterModel.InventoryModel.Items.FirstOrDefault(i =>
                i.CatalogItemModelId == ItemCatalogIds.AMMO_SHOTGUN);
        if (shotgunAmo != null)
        {
            shotgunAmo.Amount = ammoData.ShotgunAmmo;
            if (shotgunAmo.Amount <= 0)
            {
                await _itemService.Remove(shotgunAmo);
            }
            else
            {
                await _itemService.Update(shotgunAmo);
            }
        }

        var lmgAmmo =
            player.CharacterModel.InventoryModel.Items.FirstOrDefault(i =>
                i.CatalogItemModelId == ItemCatalogIds.AMMO_LIGHT_MACHINE_GUN);
        if (lmgAmmo != null)
        {
            lmgAmmo.Amount = ammoData.LightMachineGunAmmo;
            if (lmgAmmo.Amount <= 0)
            {
                await _itemService.Remove(lmgAmmo);
            }
            else
            {
                await _itemService.Update(lmgAmmo);
            }
        }

        var baseballAmmo =
            player.CharacterModel.InventoryModel.Items.FirstOrDefault(i =>
                i.CatalogItemModelId == ItemCatalogIds.WEAPON_BASEBALL);
        if (baseballAmmo != null)
        {
            baseballAmmo.Amount = ammoData.BaseballAmmo;
            if (baseballAmmo.Amount <= 0)
            {
                await _itemService.Remove(baseballAmmo);
            }
            else
            {
                await _itemService.Update(baseballAmmo);
            }
        }

        var bzGasAmmo =
            player.CharacterModel.InventoryModel.Items.FirstOrDefault(i =>
                i.CatalogItemModelId == ItemCatalogIds.WEAPON_BZ_GAS);
        if (bzGasAmmo != null)
        {
            bzGasAmmo.Amount = ammoData.BzgasAmmo;
            if (bzGasAmmo.Amount <= 0)
            {
                await _itemService.Remove(bzGasAmmo);
            }
            else
            {
                await _itemService.Update(bzGasAmmo);
            }
        }

        var flareAmmo =
            player.CharacterModel.InventoryModel.Items.FirstOrDefault(i =>
                i.CatalogItemModelId == ItemCatalogIds.WEAPON_FLARE);
        if (flareAmmo != null)
        {
            flareAmmo.Amount = ammoData.FlareAmmo;
            if (flareAmmo.Amount <= 0)
            {
                await _itemService.Remove(flareAmmo);
            }
            else
            {
                await _itemService.Update(flareAmmo);
            }
        }

        var grenadeAmmo =
            player.CharacterModel.InventoryModel.Items.FirstOrDefault(i =>
                i.CatalogItemModelId == ItemCatalogIds.WEAPON_GRENADE);
        if (grenadeAmmo != null)
        {
            grenadeAmmo.Amount = ammoData.GrenadeAmmo;
            if (grenadeAmmo.Amount <= 0)
            {
                await _itemService.Remove(grenadeAmmo);
            }
            else
            {
                await _itemService.Update(grenadeAmmo);
            }
        }

        var molotovAmmo = player.CharacterModel.InventoryModel.Items.FirstOrDefault(i =>
            i.CatalogItemModelId == ItemCatalogIds.WEAPON_MOLOTOV_COCKTAIL);
        if (molotovAmmo != null)
        {
            molotovAmmo.Amount = ammoData.MolotovAmmo;
            if (molotovAmmo.Amount <= 0)
            {
                await _itemService.Remove(molotovAmmo);
            }
            else
            {
                await _itemService.Update(molotovAmmo);
            }
        }

        var snowballAmmo =
            player.CharacterModel.InventoryModel.Items.FirstOrDefault(i =>
                i.CatalogItemModelId == ItemCatalogIds.WEAPON_SNOWBALL);
        if (snowballAmmo != null)
        {
            snowballAmmo.Amount = ammoData.SnowballAmmo;
            if (snowballAmmo.Amount <= 0)
            {
                await _itemService.Remove(snowballAmmo);
            }
            else
            {
                await _itemService.Update(snowballAmmo);
            }

            ;
        }

        await UpdateInventoryUiAsync(player);
    }

    public async Task UpdateInventoryUIs(ServerPlayer player, InventoryModel inventoryModel)
    {
        await UpdateInventoryUiAsync(player);

        // Update the others players inventory if they have the same inventory open.
        if (inventoryModel.CharacterModelId.HasValue)
        {
            var characterPlayer = Alt.GetAllPlayers().FindPlayerByCharacterId(inventoryModel.CharacterModelId.Value);
            if (characterPlayer != null)
            {
                await UpdateInventoryUiAsync(characterPlayer);
            }
        }

        // Update house inventory if they have the same inventory open.
        if (inventoryModel.HouseModelId.HasValue)
        {
            var players = Alt.GetAllPlayers()
                .Where(p => p.Dimension == inventoryModel.HouseModelId && !p.Equals(player));
            foreach (var otherPlayers in players)
            {
                await UpdateInventoryUiAsync(otherPlayers);
            }
        }

        // Update others group inventory if they have the same inventory open.
        if (inventoryModel.GroupId.HasValue)
        {
            var group = await _groupService.GetByKey(inventoryModel.GroupId.Value);

            var players = Alt.GetAllPlayers().Where(otherPlayer =>
            {
                if (player.Equals(otherPlayer))
                {
                    return false;
                }

                var member = group.Members.Find(m => m.CharacterModelId == otherPlayer.CharacterModel.Id);
                return member != null;
            });

            foreach (var otherPLayer in players)
            {
                await UpdateInventoryUiAsync(otherPLayer);
            }
        }

        // Update others trunk inventory if they have the same inventory open.
        if (inventoryModel.VehicleModelId.HasValue)
        {
            var players = Alt.GetAllPlayers().Where(otherPlayer =>
            {
                if (player.Equals(otherPlayer))
                {
                    return false;
                }

                if (otherPlayer.GetData("INTERACT_VEHICLE_TRUNK", out int vehicleDbId))
                {
                    return vehicleDbId == inventoryModel.VehicleModelId;
                }

                return false;
            });

            foreach (var otherPLayer in players)
            {
                await UpdateInventoryUiAsync(otherPLayer);
            }
        }
    }

    private async Task<List<InventoryModel>> GetInventories(ServerPlayer player)
    {
        var inventories = new List<InventoryModel>();

        // We have to update the characters cached inventory.
        var characterInventory = await _inventoryService.GetByKey(player.CharacterModel.InventoryModel.Id);
        if (characterInventory == null)
        {
            return new List<InventoryModel>();
        }

        player.CharacterModel.InventoryModel = characterInventory;

        if (player.OpenInventories.Count != 0)
        {
            var openInventories = new List<InventoryModel>();

            foreach (var openInventoryData in player.OpenInventories)
            {
                var inventory = await _inventoryService.GetByKey(openInventoryData.InventoryId);
                if (inventory == null)
                {
                    continue;
                }

                inventory.InventoryType = openInventoryData.InventoryType;
                openInventories.Add(inventory);
            }

            inventories.AddRange(openInventories);
        }

        inventories.Add(player.CharacterModel.InventoryModel);

        // Check if user is inside an house to get the house inventory.
        if (player.Dimension != 0)
        {
            var house = await _houseService.GetByKey(player.Dimension);
            if (house != null && house.Keys.Count != 0)
            {
                var houseKeyItem =
                    player.CharacterModel.InventoryModel.Items.FirstOrDefault(i => house.Keys.Any(k => k == i.Id));

                if (houseKeyItem != null || house.CharacterModelId == player.CharacterModel.Id || player.IsAduty)
                {
                    inventories.Add(house.Inventory);
                }
            }
        }

        if (player.MloInterior != 0)
        {
            var house = await _houseService.GetByDistance(player.Position, 10);
            if (house != null && house.Keys.Count != 0)
            {
                var houseKeyItem =
                    player.CharacterModel.InventoryModel.Items.FirstOrDefault(i => house.Keys.Any(k => k == i.Id));

                if (houseKeyItem != null || house.CharacterModelId == player.CharacterModel.Id || player.IsAduty)
                {
                    inventories.Add(house.Inventory);
                }
            }
        }

        // Check if player is currently interacting with an vehicle to get the trunk inventory.
        if (player.GetData("INTERACT_VEHICLE_TRUNK", out int vehicleDbId))
        {
            var vehicle = await _vehicleService.GetByKey(vehicleDbId);
            inventories.Add(vehicle.InventoryModel);
        }

        // Check if user is in a group, and around the group house to open the group member inventory.
        foreach (var group in await _groupService.Where(g =>
                     g.Members.Any(m => m.CharacterModelId == player.CharacterModel.Id)))
        {
            var member = group.Members.FirstOrDefault(m => m.CharacterModelId == player.CharacterModel.Id);
            if (member == null)
            {
                continue;
            }

            var house = await _houseService.Find(h => h.GroupModelId == group.Id);
            if (house == null)
            {
                continue;
            }

            if (player.Dimension == 0)
            {
                if (player.Position.Distance(new Position(house.PositionX, house.PositionY, house.PositionZ)) > 5)
                {
                    continue;
                }
            }
            else if (player.Dimension != house.Id)
            {
                continue;
            }

            var inventory = await _inventoryService.Find(i =>
                i.GroupCharacterId == player.CharacterModel.Id && i.GroupId == group.Id);
            if (inventory == null)
            {
                continue;
            }

            inventories.Add(inventory);
        }

        // Open all cloth related inventories.
        foreach (var item in player.CharacterModel.InventoryModel.Items.Where(i => i is ItemClothModel))
        {
            var itemCloth = await _itemClothService.GetByKey(item.Id);

            if (itemCloth.ClothingInventoryModel != null)
            {
                inventories.Add(itemCloth.ClothingInventoryModel);
            }
        }

        return inventories;
    }
}