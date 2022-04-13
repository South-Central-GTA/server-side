using System;
using AltV.Net.Async;
using AltV.Net.Enums;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Character;
using Server.Helper;
using Server.Modules.Character;
using Server.Modules.Houses;
using Server.Modules.Inventory;
using Server.Modules.SouthCentralPoints;
using Server.Modules.Vehicles;

namespace Server.Handlers.CharacterCreator;

public class CreateCharacterHandler : ISingletonScript
{
    private readonly Serializer _serializer;
    private readonly CharacterCreationModule _characterCreationModule;
    private readonly CharacterSelectionModule _characterSelectionModule;
    private readonly CharacterSpawnModule _characterSpawnModule;
    private readonly HouseModule _houseModule;
    private readonly ItemCreationModule _itemCreationModule;
    private readonly SouthCentralPointsModule _southCentralPointsModule;
    private readonly VehicleModule _vehicleModule;
    private readonly CharacterService _characterService;
    
    private readonly Random _rand = new();

    public CreateCharacterHandler(Serializer serializer, 
                                  CharacterCreationModule characterCreationModule, 
                                  CharacterSelectionModule characterSelectionModule, 
                                  CharacterSpawnModule characterSpawnModule, 
                                  HouseModule houseModule, 
                                  ItemCreationModule itemCreationModule, 
                                  SouthCentralPointsModule southCentralPointsModule, 
                                  VehicleModule vehicleModule, 
                                  CharacterService characterService)
    {
        _serializer = serializer;
        _characterCreationModule = characterCreationModule;
        _characterSelectionModule = characterSelectionModule;
        _characterSpawnModule = characterSpawnModule;
        _houseModule = houseModule;
        _itemCreationModule = itemCreationModule;
        _southCentralPointsModule = southCentralPointsModule;
        _vehicleModule = vehicleModule;
        _characterService = characterService;
        
        AltAsync.OnClient<ServerPlayer, string>("charcreator:createcharacter", OnCreateCharacter);
    }

    private async void OnCreateCharacter(ServerPlayer player, string characterCreatorDataJson)
    {
        if (!player.Exists)
        {
            return;
        }

        var characterCreatorData = _serializer.Deserialize<CharacterCreatorData>(characterCreatorDataJson);

        if (await _characterService.Exists(c => c.FirstName.ToLower() + " " + c.LastName.ToLower() == characterCreatorData.CharacterModel.Name.ToLower()))
        {
            player.SendNotification("Leider hat ein anderer Charakter exakt den selben Namen. Das können wir bedingt durch einige Systeme nicht supporten.",
                                    NotificationType.ERROR);
            player.EmitLocked("charcreator:cantfinishedcreation");
            return;
        }

        var characterCosts = await _characterCreationModule.CalculateCreation(player, characterCreatorData);

        if (player.AccountModel.SouthCentralPoints < characterCosts)
        {
            player.SendNotification("Du hast nicht genug South Central Points um dir diesen Charakter leisten zu können.",
                                    NotificationType.ERROR);
            player.EmitLocked("charcreator:cantfinishedcreation");
            return;
        }

        _southCentralPointsModule.ReducePoints(player, -characterCosts, $"Charakter '{characterCreatorData.CharacterModel.Name}' gekauft");
        InitalizeCharacter(player, characterCreatorData);
    }
    
    private async void InitalizeCharacter(ServerPlayer player, CharacterCreatorData characterCreatorData)
    {
        if (!player.Exists)
        {
            return;
        }

        var spawnLocation = _characterSpawnModule.GetSpawn(characterCreatorData.SpawnId);

        #region Set character spawn location

        characterCreatorData.CharacterModel.PositionX = spawnLocation.X;
        characterCreatorData.CharacterModel.PositionY = spawnLocation.Y;
        characterCreatorData.CharacterModel.PositionZ = spawnLocation.Z;

        characterCreatorData.CharacterModel.Pitch = spawnLocation.Pitch;
        characterCreatorData.CharacterModel.Roll = spawnLocation.Roll;
        characterCreatorData.CharacterModel.Yaw = spawnLocation.Yaw;

        #endregion

        characterCreatorData.CharacterModel.AccountModelId = player.SocialClubId;

        player.CharacterModel = await _characterService.Add(new CharacterModel(characterCreatorData.CharacterModel, characterCreatorData.StartMoney));

        foreach (var item in characterCreatorData.CharacterModel.InventoryModel.Items)
        {
            await _itemCreationModule.AddItemAsync(player, item.CatalogItemModelId, item.Amount, item.Condition, item.CustomData, item.Note, true, true, false, null, ItemState.EQUIPPED);
        }

        var house = await _houseModule.GetStarterHouse(player);
        if (house != null)
        {
            await _houseModule.SetOwner(player, house);
        }

        var vehicleOrders = characterCreatorData.PurchaseOrders.FindAll(po => po.Type == CharacterCreatorPurchaseType.VEHICLE);
        foreach (var vehicleOrder in vehicleOrders)
        {
            if (vehicleOrder.OrderedVehicle != null
                && Enum.TryParse(vehicleOrder.OrderedVehicle.Model, true, out VehicleModel vehicleModel))
            {
                var location = _characterSpawnModule.GetFreeVehicleLocation(spawnLocation);

                var randomColor = _rand.Next(0, 111);

                await _vehicleModule.CreatePersistent(vehicleModel.ToString(), player, location.Position, location.Rotation, 0, randomColor, randomColor);
            }
        }

        if (characterCreatorData.HasPhone)
        {
            await _itemCreationModule.AddItemAsync(player, ItemCatalogIds.PHONE, 1);
        }

        player.SendNotification("Dein Charakter wurde erfolgreich erstellt.", NotificationType.SUCCESS);

        await _characterCreationModule.CloseAsync(player);
        await _characterSelectionModule.SelectCharacter(player, player.CharacterModel.Id);

        _houseModule.UnselectHouseInCreation(player, false);
    }
}