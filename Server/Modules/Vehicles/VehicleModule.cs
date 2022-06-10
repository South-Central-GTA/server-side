using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Microsoft.Extensions.Logging;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Character;
using Server.Database.Models.Group;
using Server.Database.Models.Inventory;
using Server.Database.Models.Vehicles;
using Server.Modules.Dump;
using Server.Modules.Inventory;
using Server.Modules.Key;
using Server.Modules.SouthCentralPoints;

namespace Server.Modules.Vehicles;

public class VehicleModule : ITransientScript
{
    private readonly InventoryModule _inventoryModule;
    private readonly ItemCreationModule _itemCreationModule;
    private readonly ItemService _itemService;
    private readonly KeyModule _keyModule;
    private readonly ILogger<VehicleModule> _logger;
    private readonly SouthCentralPointsModule _southCentralPointsModule;
    private readonly VehicleCatalogService _vehicleCatalogService;
    private readonly VehicleDumpModule _vehicleDumpModule;
    private readonly VehicleService _vehicleService;

    public VehicleModule(ILogger<VehicleModule> logger, VehicleService vehicleService,
        VehicleCatalogService vehicleCatalogService, ItemService itemService, VehicleDumpModule vehicleDumpModule,
        SouthCentralPointsModule southCentralPointsModule, InventoryModule inventoryModule,
        ItemCreationModule itemCreationModule, KeyModule keyModule)
    {
        _logger = logger;

        _vehicleService = vehicleService;
        _vehicleCatalogService = vehicleCatalogService;
        _itemService = itemService;

        _vehicleDumpModule = vehicleDumpModule;
        _southCentralPointsModule = southCentralPointsModule;
        _inventoryModule = inventoryModule;
        _itemCreationModule = itemCreationModule;
        _keyModule = keyModule;
    }

    /// <summary>
    ///     Create a new persistent vehicle for the given player.
    /// </summary>
    public async Task<ServerVehicle?> CreatePersistent(string vehicleModel, CharacterModel character, Position position,
        Rotation rotation, int dimension, int primColor, int secColor)
    {
        var catalogVehicle = await _vehicleCatalogService.GetByKey(vehicleModel.ToLower());
        if (catalogVehicle == null)
        {
            _logger.LogCritical("Can't find catalog vehicle, can't create persistent vehicle.");
            return null;
        }

        var itemKey = (ItemKeyModel)await _itemCreationModule.AddItemAsync(character.InventoryModel, ItemCatalogIds.KEY, 1, null, null,
            "Schlüssel für " + catalogVehicle.DisplayName);
        if (itemKey == null)
        {
            return null;
        }

        var vehicleDbo = await _vehicleService.Add(new PlayerVehicleModel
        {
            BodyHealth = 1000,
            EngineHealth = 1000,
            Model = vehicleModel.ToLower(),
            CharacterModelId = character.Id,
            Position = position,
            Rotation = rotation,
            Dimension = dimension,
            PrimaryColor = primColor,
            SecondaryColor = secColor,
            Price = catalogVehicle.Price,
            Fuel = catalogVehicle.MaxTank,
            Keys = new List<int> { itemKey.Id },
            NumberplateText = await GetRandomNumberplate(),
            InventoryModel = new InventoryModel
            {
                Name = catalogVehicle.DisplayName + " Kofferraum",
                InventoryType = InventoryType.VEHICLE,
                MaxWeight = GetTrunkSize(vehicleModel)
            }
        });

        itemKey.PlayerVehicleModelId = vehicleDbo.Id;
        await _itemService.Update(itemKey);

        return await Create(vehicleDbo);
    }

    public async Task CreatePersistent(string vehicleModel, GroupModel groupModel, Position position, Rotation rotation,
        int dimension, int primColor, int secColor, VehicleState vehicleState = VehicleState.SPAWNED,
        float drivenKilometre = 0)
    {
        var catalogVehicle = await _vehicleCatalogService.GetByKey(vehicleModel.ToLower());
        if (catalogVehicle == null)
        {
            _logger.LogCritical("Can't find catalog vehicle, can't create persistent vehicle.");
            return;
        }

        var vehicleDbo = await _vehicleService.Add(new PlayerVehicleModel
        {
            BodyHealth = 1000,
            EngineHealth = 1000,
            Model = vehicleModel.ToLower(),
            GroupModelOwnerId = groupModel.Id,
            VehicleState = vehicleState,
            Position = position,
            Rotation = rotation,
            Dimension = dimension,
            PrimaryColor = primColor,
            SecondaryColor = secColor,
            Price = catalogVehicle.Price,
            Fuel = catalogVehicle.MaxTank,
            DrivenKilometre = drivenKilometre,
            NumberplateText = await GetRandomNumberplate(),
            Keys = new List<int>(),
            InventoryModel = new InventoryModel
            {
                Name = catalogVehicle.DisplayName + " Kofferraum",
                InventoryType = InventoryType.VEHICLE,
                MaxWeight = GetTrunkSize(vehicleModel)
            }
        });

        await Create(vehicleDbo);
    }

    public async Task Update(PlayerVehicleModel vehicleModel)
    {
        await _vehicleService.Update(vehicleModel);
    }

    public async Task<string> GetRandomNumberplate()
    {
        const int maxTries = 10;
        var tries = 0;

        var numberPlate = CreateNumberplate();

        while (await _vehicleService.Exists(vehicle => vehicle.NumberplateText == numberPlate))
        {
            numberPlate = CreateNumberplate();
            tries++;

            if (tries >= maxTries)
            {
                return "";
            }
        }

        return numberPlate;
    }

    public async Task<ServerVehicle?> Create(PlayerVehicleModel vehicleModel)
    {
        var vehicle = await Create(vehicleModel.Model,
            new Position(vehicleModel.PositionX, vehicleModel.PositionY, vehicleModel.PositionZ),
            new Rotation(vehicleModel.Roll, vehicleModel.Pitch, vehicleModel.Yaw), vehicleModel.PrimaryColor,
            vehicleModel.SecondaryColor, vehicleModel.Livery, vehicleModel.BodyHealth, vehicleModel.EngineHealth,
            vehicleModel.Fuel, vehicleModel.DrivenKilometre);

        if (vehicle == null)
        {
            return null;
        }

        vehicle.NumberplateText = vehicleModel.NumberplateText;

        vehicle.LockState = vehicleModel.LockState == LockState.OPEN
            ? VehicleLockState.Unlocked
            : VehicleLockState.Locked;

        vehicle.DbEntity = vehicleModel;

        await _vehicleService.Update(vehicleModel);

        await SetSyncedDataAsync(vehicle);

        return vehicle;
    }

    public async Task<ServerVehicle?> Create(string vehicleModel, Position position, Rotation rotation,
        int primaryColor, int secondaryColor, byte livery = 0, uint bodyHealth = 1000, int engineHealth = 1000,
        float fuel = 0, float drivenKilometre = 0)
    {
        var catalogVehicle = await _vehicleCatalogService.Find(vc => vc.Model.ToLower() == vehicleModel.ToLower());
        if (catalogVehicle == null)
        {
            return null;
        }

        ServerVehicle? vehicle = new(Alt.Hash(vehicleModel), position, rotation)
        {
            PrimaryColor = byte.Parse(primaryColor.ToString()),
            SecondaryColor = byte.Parse(secondaryColor.ToString()),
            Livery = livery,
            ModKit = 1,
            BodyHealth = bodyHealth,
            EngineHealth = engineHealth,
            Fuel = fuel,
            DrivenKilometre = drivenKilometre
        };

        return vehicle;
    }

    public async Task SetVehicleFuel(ServerPlayer player, ServerVehicle vehicle, int amount)
    {
        if (vehicle.DbEntity == null)
        {
            player.SendNotification("Dieses Fahrzeug kann nicht bedankt werden.", NotificationType.ERROR);
            return;
        }

        vehicle.Fuel = amount;

        await Save(vehicle);
        await SetSyncedDataAsync(vehicle);
    }

    public async Task SetSyncedDataAsync(ServerVehicle vehicle)
    {
        if (vehicle.DbEntity == null)
        {
            return;
        }

        var dbEntry = await _vehicleService.GetByKey(vehicle.DbEntity.Id);
        var catalogVehicle = await _vehicleCatalogService.GetByKey(dbEntry.Model.ToLower());
        if (catalogVehicle == null)
        {
            return;
        }

        var lockState = dbEntry.LockState == LockState.CLOSED ? "Abgeschlossen" : "Aufgeschlossen";
        var ownerLabel = "Kein Eigentümer";

        if (dbEntry.CharacterModel != null)
        {
            ownerLabel = dbEntry.CharacterModel.Name;
        }

        if (dbEntry.GroupModelOwner != null)
        {
            ownerLabel = dbEntry.GroupModelOwner.Name;
        }

        vehicle.SetSyncedMetaData("ID", dbEntry.Id);
        vehicle.SetSyncedMetaData("OWNER", ownerLabel);
        vehicle.SetSyncedMetaData("OWNER_CHARACTER_ID", dbEntry.CharacterModelId ?? -1);
        vehicle.SetSyncedMetaData("OWNER_GROUP_ID", dbEntry.GroupModelOwnerId ?? -1);
        vehicle.SetSyncedMetaData("LOCK_STATE", lockState);
        vehicle.SetSyncedMetaData("MAX_FUEL", catalogVehicle.MaxTank);
    }

    public async Task CreateVehicleKey(ServerPlayer player, ServerVehicle vehicle)
    {
        if (vehicle.DbEntity == null)
        {
            return;
        }

        var catalogVehicle = await _vehicleCatalogService.GetByKey(vehicle.DbEntity.Model.ToLower());
        if (catalogVehicle == null)
        {
            return;
        }

        var itemKey = (ItemKeyModel)await _itemCreationModule.AddItemAsync(player, ItemCatalogIds.KEY, 1, null, null,
            "Schlüssel für " + catalogVehicle.DisplayName);
        if (itemKey == null)
        {
            return;
        }

        vehicle.DbEntity.Keys ??= new List<int>();

        vehicle.DbEntity.Keys.Add(itemKey.Id);

        await _itemService.Update(itemKey);
        await _inventoryModule.UpdateInventoryUiAsync(player);

        await _vehicleService.Update(vehicle.DbEntity);
    }

    public async Task SetCharacterOwner(ServerVehicle vehicle, ServerPlayer player)
    {
        // When the player bought this vehicle from another group.
        if (vehicle.DbEntity.GroupModelOwnerId.HasValue)
        {
            await CreateVehicleKey(player, vehicle);
        }

        vehicle.DbEntity.GroupModelOwnerId = null;
        vehicle.DbEntity.CharacterModelId = player.CharacterModel.Id;

        await _vehicleService.Update(vehicle.DbEntity);

        await SetSyncedDataAsync(vehicle);
    }

    private static string CreateNumberplate()
    {
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var rnd = new Random();
        var stringBuilder = new StringBuilder(7);

        stringBuilder.Append(rnd.Next(1, 9));

        for (var i = 0; i < 3; i++)
        {
            var num = rnd.Next(0, chars.Length - 1);
            stringBuilder.Append(chars[num]);
        }

        stringBuilder.Append(rnd.Next(1000, 9999));

        return stringBuilder.ToString();
    }

    public async Task Save(ServerVehicle vehicle)
    {
        if (vehicle.DbEntity == null)
        {
            return;
        }

        vehicle.DbEntity.EngineOn = vehicle.EngineOn;
        vehicle.DbEntity.Position = vehicle.Position;
        vehicle.DbEntity.Rotation = vehicle.Rotation;
        vehicle.DbEntity.Dimension = vehicle.Dimension;
        vehicle.DbEntity.EngineHealth = vehicle.EngineHealth;
        vehicle.DbEntity.BodyHealth = vehicle.BodyHealth;
        vehicle.DbEntity.Fuel = vehicle.Fuel;
        vehicle.DbEntity.DrivenKilometre = vehicle.DrivenKilometre;

        if (vehicle.LockState == VehicleLockState.Locked)
        {
            vehicle.DbEntity.LockState = LockState.CLOSED;
        }

        if (vehicle.LockState == VehicleLockState.Unlocked)
        {
            vehicle.DbEntity.LockState = LockState.OPEN;
        }

        if (vehicle.LockState == VehicleLockState.None)
        {
            vehicle.DbEntity.LockState = LockState.BROKEN;
        }

        await _vehicleService.Update(vehicle.DbEntity);
    }


    public async Task SaveRange(List<ServerVehicle> vehicles)
    {
        var vehiclesToUpdate = new List<PlayerVehicleModel>();
        foreach (var vehicle in vehicles)
        {
            if (vehicle.DbEntity == null)
            {
                continue;
            }

            vehicle.DbEntity.EngineOn = vehicle.EngineOn;
            vehicle.DbEntity.Position = vehicle.Position;
            vehicle.DbEntity.Rotation = vehicle.Rotation;
            vehicle.DbEntity.Dimension = vehicle.Dimension;
            vehicle.DbEntity.EngineHealth = vehicle.EngineHealth;
            vehicle.DbEntity.BodyHealth = vehicle.BodyHealth;
            vehicle.DbEntity.Fuel = vehicle.Fuel;
            vehicle.DbEntity.DrivenKilometre = vehicle.DrivenKilometre;

            if (vehicle.LockState == VehicleLockState.Locked)
            {
                vehicle.DbEntity.LockState = LockState.CLOSED;
            }

            if (vehicle.LockState == VehicleLockState.Unlocked)
            {
                vehicle.DbEntity.LockState = LockState.OPEN;
            }

            if (vehicle.LockState == VehicleLockState.None)
            {
                vehicle.DbEntity.LockState = LockState.BROKEN;
            }

            vehiclesToUpdate.Add(vehicle.DbEntity);
        }

        await _vehicleService.UpdateRange(vehiclesToUpdate);
    }

    public async Task SetEngineState(ServerVehicle vehicle, ServerPlayer player, bool state, bool force = false)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.IsInVehicle)
        {
            player.SendNotification("Dein Charakter muss in einem Fahrzeug sitzen.", NotificationType.ERROR);
            return;
        }

        if (player.Seat != 1)
        {
            player.SendNotification("Du musst Fahrer des Fahrzeug sein.", NotificationType.ERROR);
            return;
        }

        if (!force)
        {
            var returnVal = await CanToggleEngine(player, vehicle, !vehicle.EngineOn);
            switch (returnVal)
            {
                case EngineErrorType.NO_ENGINE:
                    player.SendNotification("Das Fahrzeug hat kein Motor.", NotificationType.ERROR);
                    return;

                case EngineErrorType.ENGINE_DAMAGED:
                    player.SendNotification("Der Motor des Fahrzeuges ist beschädigt.", NotificationType.ERROR);
                    return;

                case EngineErrorType.NO_FUEL:
                    player.SendNotification("Der Motor hat kein Treibstoff.", NotificationType.ERROR);
                    return;

                case EngineErrorType.NOT_SPAWNED:
                    player.SendNotification("Das Fahrzeug ist nicht korrekt gespawnt.", NotificationType.ERROR);
                    return;

                case EngineErrorType.NO_KEY:
                    player.SendNotification("Dein Charakter hat keinen passenden Schlüssel.", NotificationType.ERROR);
                    return;
            }
        }

        await vehicle.SetEngineOnAsync(state);

        var stateString = state ? "gestartet" : "gestoppt";
        player.SendNotification($"Du hast den Motor {stateString}.", NotificationType.INFO);
    }

    public async Task<EngineErrorType> CanToggleEngine(ServerPlayer player, ServerVehicle vehicle, bool nextState)
    {
        if (!HasEngine(vehicle))
        {
            return EngineErrorType.NO_ENGINE;
        }

        if (nextState)
        {
            if (vehicle.EngineHealth < 300)
            {
                return EngineErrorType.ENGINE_DAMAGED;
            }
        }

        if (vehicle.DbEntity == null)
        {
            return EngineErrorType.SUCCESS;
        }

        if (nextState)
        {
            if (vehicle.Fuel <= 0)
            {
                return EngineErrorType.NO_FUEL;
            }
        }

        if (vehicle.DbEntity.VehicleState != VehicleState.SPAWNED)
        {
            return EngineErrorType.NOT_SPAWNED;
        }

        switch (await _keyModule.HasKey(player, vehicle.DbEntity))
        {
            case HasKeyErrorType.HAS_NO_KEY:
            case HasKeyErrorType.HAS_WRONG_GROUP_KEY:
                return EngineErrorType.NO_KEY;
        }

        return EngineErrorType.SUCCESS;
    }

    public bool HasEngine(IVehicle vehicle)
    {
        var hasEngine = true;

        var dumpEntry = _vehicleDumpModule.Dump.Find(vd => vd.Hash == vehicle.Model);
        if (dumpEntry is { Class: "CYCLE" })
        {
            hasEngine = false;
        }

        return hasEngine;
    }

    public async Task<int> GetPointsPrice(string model)
    {
        var catalogVehicle = await _vehicleCatalogService.GetByKey(model.ToLower());
        if (catalogVehicle == null)
        {
            _logger.LogCritical($"No vehicle catalog was found for {model}, used 999999 for security.");
            return 999999;
        }

        return _southCentralPointsModule.GetPointsPrice(catalogVehicle.Price);
    }

    private float GetTrunkSize(string model)
    {
        float trunkSize = 30;

        if (Enum.TryParse(model, out VehicleModel vehicleModel))
        {
            var vehicleDumpEntry = _vehicleDumpModule.Dump.Find(vd => vd.Hash == (long)vehicleModel);
            if (vehicleDumpEntry == null)
            {
                _logger.LogCritical(
                    $"No vehicle dump data was found for model: {model}, so default inventory size used for trunk.");
                return trunkSize;
            }

            switch (vehicleDumpEntry.Class)
            {
                // Trunk size based on vehicle class
                case "COMMERCIAL":
                case "INDUSTRIAL":
                case "HELICOPTER":
                    trunkSize = 200;
                    break;
                case "VAN":
                case "SUV":
                case "OFF_ROAD":
                case "EMERGENCY":
                    trunkSize = 100;
                    break;
                case "SPORT":
                case "SPORT_CLASSIC":
                    trunkSize = 20;
                    break;
                case "SUPER":
                case "BOAT":
                case "COMPACT":
                    trunkSize = 25;
                    break;
                case "MOTORCYCLE":
                    trunkSize = 10;
                    break;
            }

            switch (vehicleDumpEntry.Name)
            {
                // More specialized trunk size get ajusted by vehicle
                case "blazer":
                case "blazer1":
                case "blazer2":
                case "blazer3":
                case "blazer4":
                case "blazer5":
                    trunkSize = 10;
                    break;
                case "police":
                case "police2":
                case "police3":
                case "police4":
                case "sheriff":
                    trunkSize = 40;
                    break;
                case "policeb":
                case "predator":
                case "polmav":
                    trunkSize = 20;
                    break;
            }
        }
        else
        {
            // modded vehicle support
            switch (model)
            {
                case "pscout":
                case "bcat":
                case "firegranger2":
                case "lsiagranger":
                case "beachp":
                case "umkscout":
                case "lsfd3":
                case "lsfd4":
                case "lsfd5":
                    trunkSize = 50;
                    break;
                case "lsfd2":
                    trunkSize = 60;
                    break;
                case "lsfdtruck":
                case "lsfdtruck2":
                case "lsfdtruck3":
                case "ladder":
                    trunkSize = 100;
                    break;
            }
        }


        return trunkSize;
    }
}