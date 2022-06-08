using System;
using System.Threading.Tasks;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Server.Core;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Configuration;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.DataAccessLayer.Services;
using Server.Database.Models.Housing;
using Server.Database.Models.Inventory;
using Server.Modules.EntitySync;
using Server.Modules.Houses;
using Server.Modules.Inventory;

namespace Server.Handlers.Session;

public class PlayerDisconnectHandler : ISingletonScript
{
    private readonly CharacterService _characterService;
    private readonly DeliveryService _deliveryService;
    private readonly HouseModule _houseModule;
    private readonly HouseService _houseService;
    private readonly FileService _fileService;

    private readonly PedSyncModule _pedSyncModule;

    public PlayerDisconnectHandler(
        CharacterService characterService,
        DeliveryService deliveryService,
        HouseService houseService,
        HouseModule houseModule,
        FileService fileService,
        PedSyncModule pedSyncModule)
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

            player.CharacterModel.Dimension = player.Dimension;
            player.CharacterModel.Health = player.Health;
            player.CharacterModel.Armor = player.Armor;

            player.EmitLocked("charcreator:reset");
            player.EmitLocked("charselector:reset");
            player.EmitLocked("hairsalon:reset");
            player.EmitLocked("clothingstore:reset");
            player.EmitLocked("tattoostudio:reset");

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