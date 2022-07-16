﻿using System;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Models.Housing;
using Server.Modules.EntitySync;
using Server.Modules.Houses;

namespace Server.Handlers.Session;

public class PlayerDisconnectHandler : ISingletonScript
{
    private readonly CharacterService _characterService;
    private readonly DeliveryService _deliveryService;
    private readonly FileService _fileService;
    private readonly HouseModule _houseModule;
    private readonly HouseService _houseService;

    private readonly PedSyncModule _pedSyncModule;

    public PlayerDisconnectHandler(CharacterService characterService, DeliveryService deliveryService,
        HouseService houseService, HouseModule houseModule, FileService fileService, PedSyncModule pedSyncModule)
    {
        _characterService = characterService;
        _deliveryService = deliveryService;
        _houseService = houseService;
        _fileService = fileService;

        _houseModule = houseModule;
        _pedSyncModule = pedSyncModule;

        AltAsync.OnPlayerDisconnect += (player, reason) =>
            OnPlayerDisconnect(player as ServerPlayer ?? throw new InvalidOperationException(), reason);
    }

    private async Task OnPlayerDisconnect(ServerPlayer player, string reason)
    {
        if (player.Exists && player.IsSpawned && player.AccountModel.AdminCheckpoints == 0)
        {
            player.CharacterModel.PositionX = player.Position.X;
            player.CharacterModel.PositionY = player.Position.Y;
            player.CharacterModel.PositionZ = player.Position.Z;

            player.CharacterModel.Roll = player.Rotation.Roll;
            player.CharacterModel.Pitch = player.Rotation.Pitch;
            player.CharacterModel.Yaw = player.Rotation.Yaw;
            
            // Reset dimension if necessary.
            if (player.GetData("VEHICLE_SERVICE_DATA", out VehicleServiceData vehicleServiceData))
            {
                player.Dimension = 0;
                
                var vehicle = Alt.GetAllVehicles().FindByDbId(vehicleServiceData.VehicleDbId);
                if (vehicle is { Exists: true })
                {
                    vehicle.Dimension = 0;
                }
            }

            player.CharacterModel.Dimension = player.Dimension;
            player.CharacterModel.Health = player.Health;
            player.CharacterModel.Armor = player.Armor;

            if (player.IsDuty)
            {
                if (await _houseService.GetByKey(player.DutyLeaseCompanyHouseId) is LeaseCompanyHouseModel
                    leaseCompanyHouse)
                {
                    leaseCompanyHouse.PlayerDuties--;

                    if (leaseCompanyHouse.PlayerDuties <= 0)
                    {
                        leaseCompanyHouse.PlayerDuties = 0;

                        if (leaseCompanyHouse.HasCashier)
                        {
                            _pedSyncModule.CreateCashier(leaseCompanyHouse);
                        }
                    }

                    await _houseService.Update(leaseCompanyHouse);
                }
            }

            await _characterService.Update(player);

            var delivery = await _deliveryService.Find(d => d.SupplierCharacterId == player.CharacterModel.Id);
            if (delivery != null)
            {
                delivery.OldStatus = delivery.Status;
                await _deliveryService.Update(delivery);
            }

            var file = await _fileService.Find(f => f.IsBlocked && f.BlockedByCharacterName != null &&
                                                    f.BlockedByCharacterName == player.CharacterModel.Name);
            if (file != null)
            {
                file.IsBlocked = false;
                await _fileService.Update(file);
            }
        }

        _houseModule.UnselectHouseInCreation(player, false);
        _pedSyncModule.Delete(player);

        player.ClearData();
        player.ClearAllTimer();
    }
}