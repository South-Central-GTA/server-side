using System;
using System.Threading.Tasks;
using AltV.Net.Async;
using AltV.Net.Enums;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models;
using Server.Database.Models.Character;
using Server.Database.Models.Inventory;
using Server.Helper;
using Server.Modules.Character;
using Server.Modules.Clothing;
using Server.Modules.Houses;
using Server.Modules.Inventory;
using Server.Modules.SouthCentralPoints;
using Server.Modules.Vehicles;

namespace Server.Handlers.CharacterCreator;

public class CreateCharacterHandler : ISingletonScript
{
    private readonly CharacterCreationModule _characterCreationModule;
    private readonly CharacterSelectionModule _characterSelectionModule;
    private readonly CharacterService _characterService;
    private readonly CharacterSpawnModule _characterSpawnModule;
    private readonly HouseModule _houseModule;
    private readonly ItemCreationModule _itemCreationModule;
    private readonly ClothingItemCreationModule _clothingItemCreationModule;
    private readonly PersonalLicenseService _personalLicenseService;

    private readonly Random _rand = new();
    private readonly RegistrationOfficeService _registrationOfficeService;
    private readonly Serializer _serializer;
    private readonly SouthCentralPointsModule _southCentralPointsModule;
    private readonly VehicleModule _vehicleModule;

    public CreateCharacterHandler(Serializer serializer, CharacterCreationModule characterCreationModule,
        CharacterSelectionModule characterSelectionModule, CharacterSpawnModule characterSpawnModule,
        HouseModule houseModule, ItemCreationModule itemCreationModule,
        SouthCentralPointsModule southCentralPointsModule, VehicleModule vehicleModule,
        CharacterService characterService, RegistrationOfficeService registrationOfficeService,
        PersonalLicenseService personalLicenseService, ClothingItemCreationModule clothingItemCreationModule)
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
        _registrationOfficeService = registrationOfficeService;
        _personalLicenseService = personalLicenseService;
        _clothingItemCreationModule = clothingItemCreationModule;

        AltAsync.OnClient<ServerPlayer, string>("charcreator:createcharacter", OnCreateCharacter);
    }

    private async void OnCreateCharacter(ServerPlayer player, string characterCreatorDataJson)
    {
        if (!player.Exists)
        {
            return;
        }

        var characterCreatorData = _serializer.Deserialize<CharacterCreatorData>(characterCreatorDataJson);

        if (await _characterService.Exists(c =>
                c.FirstName.ToLower() + " " + c.LastName.ToLower() ==
                characterCreatorData.CharacterModel.Name.ToLower()))
        {
            player.SendNotification(
                "Leider hat ein anderer Charakter exakt den selben Namen. Das können wir bedingt durch einige Systeme nicht supporten.",
                NotificationType.ERROR);
            player.EmitLocked("charcreator:cantfinishedcreation");
            return;
        }

        var characterCosts = await _characterCreationModule.CalculateCreation(player, characterCreatorData);

        if (player.AccountModel.SouthCentralPoints < characterCosts)
        {
            player.SendNotification(
                "Du hast nicht genug South Central Points um dir diesen Charakter leisten zu können.",
                NotificationType.ERROR);
            player.EmitLocked("charcreator:cantfinishedcreation");
            return;
        }

        _southCentralPointsModule.ReducePoints(player, -characterCosts,
            $"Charakter '{characterCreatorData.CharacterModel.Name}' gekauft");
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

        var newCharacter =
            await _characterService.Add(new CharacterModel(characterCreatorData.CharacterModel,
                characterCreatorData.StartMoney));

        await GiveClothingItems(newCharacter, characterCreatorData);
        
        var house = await _houseModule.GetStarterHouse(player);
        if (house != null)
        {
            await _houseModule.SetOwner(newCharacter, house);
        }

        var vehicleOrders =
            characterCreatorData.PurchaseOrders.FindAll(po => po.Type == CharacterCreatorPurchaseType.VEHICLE);

        foreach (var vehicleOrder in vehicleOrders)
        {
            if (vehicleOrder.OrderedVehicle != null &&
                Enum.TryParse(vehicleOrder.OrderedVehicle.Model, true, out VehicleModel vehicleModel))
            {
                var location = _characterSpawnModule.GetFreeVehicleLocation(spawnLocation);

                var randomColor = _rand.Next(0, 111);

                await _vehicleModule.CreatePersistent(vehicleModel.ToString(), newCharacter, location.Position,
                    location.Rotation, 0, randomColor, randomColor);
            }
        }

        if (characterCreatorData.HasPhone)
        {
            await _itemCreationModule.AddItemAsync(newCharacter.InventoryModel, ItemCatalogIds.PHONE, 1);
        }

        if (characterCreatorData.IsRegistered)
        {
            await _registrationOfficeService.Add(
                new RegistrationOfficeEntryModel { CharacterModelId = newCharacter.Id });
        }

        if (characterCreatorData.HasDrivingLicense)
        {
            await _personalLicenseService.Add(new PersonalLicenseModel
            {
                CharacterModelId = newCharacter.Id, Type = PersonalLicensesType.DRIVING
            });

            await _itemCreationModule.AddItemAsync(newCharacter.InventoryModel, ItemCatalogIds.LICENSES, 1, newCharacter.Id.ToString());
        }

        player.SendNotification("Dein Charakter wurde erfolgreich erstellt.", NotificationType.SUCCESS);

        await _characterCreationModule.CloseAsync(player);
        await _characterSelectionModule.SelectCharacter(player, newCharacter.Id);

        _houseModule.UnselectHouseInCreation(player, false);
    }

    private async Task GiveClothingItems(CharacterModel character, CharacterCreatorData characterCreatorData)
    {
        var hat = characterCreatorData.ClothingsData.Hat;
        if (hat != null)
        {
            await _clothingItemCreationModule.AddItemAsync(character.InventoryModel, ItemCatalogIds.CLOTHING_HAT, hat);
        }
        
        var glasses = characterCreatorData.ClothingsData.Glasses;
        if (glasses != null)
        {
            await _clothingItemCreationModule.AddItemAsync(character.InventoryModel, ItemCatalogIds.CLOTHING_GLASSES, glasses);
        }
        
        var ears = characterCreatorData.ClothingsData.Ears;
        if (ears != null)
        {
            await _clothingItemCreationModule.AddItemAsync(character.InventoryModel, ItemCatalogIds.CLOTHING_EARS, ears);
        }
        
        var watch = characterCreatorData.ClothingsData.Watch;
        if (watch != null)
        {
            await _clothingItemCreationModule.AddItemAsync(character.InventoryModel, ItemCatalogIds.CLOTHING_WATCH, watch);
        }
        
        var bracelets = characterCreatorData.ClothingsData.Bracelets;
        if (bracelets != null)
        {
            await _clothingItemCreationModule.AddItemAsync(character.InventoryModel, ItemCatalogIds.CLOTHING_BRACELET, bracelets);
        }
        
        var mask = characterCreatorData.ClothingsData.Mask;
        if (mask != null)
        {
            await _clothingItemCreationModule.AddItemAsync(character.InventoryModel, ItemCatalogIds.CLOTHING_MASK, mask);
        }
        
        var top = characterCreatorData.ClothingsData.Top;
        if (top != null)
        {
            await _clothingItemCreationModule.AddItemAsync(character.InventoryModel, ItemCatalogIds.CLOTHING_TOP, top);
        }
        
        var bodyArmor = characterCreatorData.ClothingsData.BodyArmor;
        if (bodyArmor != null)
        {
            await _clothingItemCreationModule.AddItemAsync(character.InventoryModel, ItemCatalogIds.CLOTHING_BODY_ARMOR, bodyArmor);
        }
        
        var backPack = characterCreatorData.ClothingsData.BackPack;
        if (backPack != null)
        {
            await _clothingItemCreationModule.AddItemAsync(character.InventoryModel, ItemCatalogIds.CLOTHING_BACKPACK, backPack);
        }
        
        var underShirt = characterCreatorData.ClothingsData.UnderShirt;
        if (underShirt != null)
        {
            await _clothingItemCreationModule.AddItemAsync(character.InventoryModel, ItemCatalogIds.CLOTHING_UNDERSHIRT, underShirt);
        }
        
        var accessories = characterCreatorData.ClothingsData.Accessories;
        if (accessories != null)
        {
            await _clothingItemCreationModule.AddItemAsync(character.InventoryModel, ItemCatalogIds.CLOTHING_ACCESSORIES, accessories);
        }
        
        var pants = characterCreatorData.ClothingsData.Pants;
        if (pants != null)
        {
            await _clothingItemCreationModule.AddItemAsync(character.InventoryModel, ItemCatalogIds.CLOTHING_PANTS, pants);
        }
        
        var shoes = characterCreatorData.ClothingsData.Shoes;
        if (shoes != null)
        {
            await _clothingItemCreationModule.AddItemAsync(character.InventoryModel, ItemCatalogIds.CLOTHING_SHOES, shoes);
        }
    }
}