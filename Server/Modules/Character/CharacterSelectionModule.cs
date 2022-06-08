using System;
using System.Threading.Tasks;
using AltV.Net.Async;
using AltV.Net.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Configuration;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.Bank;
using Server.Modules.Chat;
using Server.Modules.EntitySync;
using Server.Modules.Group;
using Server.Modules.Houses;
using Server.Modules.Prison;
using Server.Modules.Vehicles;

namespace Server.Modules.Character;

public class CharacterSelectionModule
    : ITransientScript
{
    private readonly ILogger<CharacterSelectionModule> _logger;
    private readonly WorldLocationOptions _worldLocationOptions;

    private readonly AccountService _accountService;
    private readonly CharacterService _characterService;
    private readonly DeliveryService _deliveryService;
    private readonly HouseService _houseService;

    private readonly BankModule _bankModule;
    private readonly CharacterSpawnModule _characterSpawnModule;
    private readonly CommandModule _commandModule;
    private readonly GroupModule _groupModule;
    private readonly HouseModule _houseModule;
    private readonly VehicleLocatingModule _vehicleLocatingModule;
    private readonly PrisonModule _prisonModule;
    private readonly PedSyncModule _pedSyncModule;

    public CharacterSelectionModule(
        ILogger<CharacterSelectionModule> logger,
        IOptions<WorldLocationOptions> worldLocationOptions,
        CharacterService characterService,
        DeliveryService deliveryService,
        HouseService houseService,
        AccountService accountService,
        CharacterSpawnModule characterSpawnModule,
        HouseModule houseModule,
        CommandModule commandModule,
        BankModule bankModule,
        GroupModule groupModule,
        VehicleLocatingModule vehicleLocatingModule,
        PrisonModule prisonModule, PedSyncModule pedSyncModule)
    {
        _logger = logger;
        _characterSpawnModule = characterSpawnModule;
        _worldLocationOptions = worldLocationOptions.Value;
        _characterService = characterService;
        _deliveryService = deliveryService;
        _houseService = houseService;
        _houseModule = houseModule;
        _accountService = accountService;
        _commandModule = commandModule;
        _bankModule = bankModule;
        _groupModule = groupModule;
        _vehicleLocatingModule = vehicleLocatingModule;
        _prisonModule = prisonModule;
        _pedSyncModule = pedSyncModule;
    }

    public async Task OpenAsync(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        player.ClearData();
        player.ClearAllTimer();

        var characters = await _characterService.GetAllFromAccount(player.AccountModel);
        characters = characters.FindAll(c => c.CharacterState == CharacterState.PLAYABLE);

        player.IsSpawned = false;
        player.IsAduty = false;
        player.IsDuty = false;
        player.SetUniqueDimension();
        player.SetPositionLocked(new Position(_worldLocationOptions.CharacterSelectionPositionX,
                                              _worldLocationOptions.CharacterSelectionPositionY,
                                              _worldLocationOptions.CharacterSelectionPositionZ));
        player.DeleteStreamSyncedMetaData("DUTY");

        _vehicleLocatingModule.StopTracking(player);
        _pedSyncModule.Delete(player);

        player.EmitLocked("charselector:open", characters, player.AccountModel.LastSelectedCharacterId);
    }

    public async Task UpdateAsync(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        var characters = await _characterService.GetAllFromAccount(player.AccountModel);
        characters = characters.FindAll(c => c.CharacterState == CharacterState.PLAYABLE);
        player.EmitLocked("charselector:update", characters, player.AccountModel.LastSelectedCharacterId);
    }

    public async Task SelectCharacter(ServerPlayer player, int characterId)
    {
        if (!player.Exists)
        {
            return;
        }

        var character = await _characterService.GetByKey(characterId);
        if (character == null)
        {
            return;
        }

        if (character.AccountModelId != player.AccountModel.SocialClubId)
        {
            _logger.LogError(
                $"{player.AccountName} tries to play {character.Name} but the character ownership is wrong.");
            return;
        }

        player.EmitLocked("charselector:close");
        player.EmitLocked("chat:setcommands", _commandModule.GetAllCommand(player));

        character.OnlineSince = DateTime.Now;
        player.CharacterModel = character;

        player.AccountModel.LastSelectedCharacterId = characterId;

        await _accountService.Update(player.AccountModel);
        await _characterService.Update(player.CharacterModel);

        await _houseModule.UpdateUi(player);
        await _bankModule.UpdateUi(player);
        await _groupModule.UpdateUi(player);

        // If the player had a delivery check if he is in the timeframe to rejoin the job
        var delivery = await _deliveryService.Find(d => d.SupplierCharacterId == character.Id);
        if (delivery != null)
        {
            if (delivery.OldStatus == DeliveryState.ACCEPTED)
            {
                player.SetWaypoint(new Position(_worldLocationOptions.HarbourSelectionPositionX,
                                                _worldLocationOptions.HarbourSelectionPositionY,
                                                _worldLocationOptions.HarbourSelectionPositionZ),
                                   5,
                                   1);
            }

            if (delivery.OldStatus == DeliveryState.LOADED)
            {
                var house = await _houseService.Find(h => h.GroupModelId == delivery.OrderGroupModelId);
                if (house == null)
                {
                    return;
                }

                player.SetWaypoint(new Position(house.PositionX, house.PositionY, house.PositionZ), 5, 1);
            }

            delivery.Status = delivery.OldStatus;

            await _deliveryService.Update(delivery);

            player.SendNotification(
                $"Der Lieferauftrag für '{delivery.OrderGroupModel.Name}' wurde gespeichert, das Ziel wurde auf der Karte markiert.",
                NotificationType.SUCCESS);
        }

        await _characterSpawnModule.Spawn(player,
                                          new Position(player.CharacterModel.PositionX,
                                                       player.CharacterModel.PositionY,
                                                       player.CharacterModel.PositionZ),
                                          new Rotation(player.CharacterModel.Roll,
                                                       player.CharacterModel.Pitch,
                                                       player.CharacterModel.Yaw),
                                          player.CharacterModel.Dimension);

        // Check if player is in jail
        if (player.CharacterModel.JailedUntil.HasValue)
        {
            if (player.CharacterModel.JailedUntil.Value < DateTime.Now)
            {
                await _prisonModule.ClearPlayerFromPrison(player);
            }
            else
            {
                _prisonModule.SetPlayerInPrison(player);
            }
        }
    }
}