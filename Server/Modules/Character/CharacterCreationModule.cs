using System;
using System.Threading.Tasks;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Configuration;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Character;
using Server.Helper;
using Server.Modules.Clothing;
using Server.Modules.Houses;
using Server.Modules.SouthCentralPoints;
using Server.Modules.Vehicles;

namespace Server.Modules.Character;

public class CharacterCreationModule
    : ITransientScript
{
    private readonly CharacterCreatorOptions _characterCreatorOptions;
    private readonly CharacterService _characterService;
    private readonly GameOptions _gameOptions;

    private readonly HouseModule _houseModule;

    private readonly ItemCatalogService _itemCatalogService;
    private readonly ILogger<CharacterCreationModule> _logger;
    private readonly Serializer _serializer;
    private readonly SouthCentralPointsModule _southCentralPointsModule;
    private readonly VehicleModule _vehicleModule;

    public CharacterCreationModule(
        IOptions<GameOptions> gameOptions,
        IOptions<CharacterCreatorOptions> characterCreatorOptions,
        ILogger<CharacterCreationModule> logger,
        Serializer serializer,
        HouseModule houseModule,
        VehicleModule vehicleModule,
        SouthCentralPointsModule southCentralPointsModule,
        ItemCatalogService itemCatalogService,
        CharacterService characterService)
    {
        _gameOptions = gameOptions.Value;
        _characterCreatorOptions = characterCreatorOptions.Value;
        _logger = logger;
        _serializer = serializer;

        _houseModule = houseModule;
        _vehicleModule = vehicleModule;
        _southCentralPointsModule = southCentralPointsModule;

        _itemCatalogService = itemCatalogService;
        _characterService = characterService;
    }

    public async Task OpenAsync(ServerPlayer player, CharacterModel characterModel)
    {
        if (!player.Exists)
        {
            return;
        }

        var characters = await _characterService.GetAllFromAccount(player.AccountModel);
        var isTutorial = characters.Count == 0;
        var phoneCatalogItem = await _itemCatalogService.GetByKey(ItemCatalogIds.PHONE);

        await AltAsync.Do(() =>
        {
            player.EmitLocked("charcreator:open",
                              characterModel,
                              isTutorial,
                              _gameOptions.MoneyToPointsExchangeRate,
                              _characterCreatorOptions.CharacterBaseCosts,
                              (int)Math.Round(phoneCatalogItem.Price * _gameOptions.MoneyToPointsExchangeRate));
            player.EmitLocked("charselector:close");
            player.SetPositionLocked(new Position(413.31427f, -997.31866f, -99.41907f));
            player.SetUniqueDimension();
        });

        _logger.LogInformation($"{player.AccountName} has joined the character creator.");
    }

    public async Task CloseAsync(ServerPlayer player)
    {
        await AltAsync.Do(() =>
        {
            if (!player.Exists)
            {
                return;
            }

            player.EmitLocked("charcreator:reset");
            player.EmitLocked("houseselector:reset");
            player.EmitLocked("spawnselector:reset");

            lock (player)
            {
                player.Dimension = 0;
            }
        });
    }

    public async Task<int> CalculateCreation(ServerPlayer player, CharacterCreatorData characterCreatorData)
    {
        var characterCosts = _characterCreatorOptions.CharacterBaseCosts;

        var house = await _houseModule.GetStarterHouse(player);
        if (house != null)
        {
            characterCosts += _southCentralPointsModule.GetPointsPrice(house.Price);
        }

        foreach (var order in characterCreatorData.PurchaseOrders)
        {
            switch (order.Type)
            {
                case CharacterCreatorPurchaseType.MONEY:
                    characterCosts += _southCentralPointsModule.GetPointsPrice(characterCreatorData.StartMoney);
                    break;
                case CharacterCreatorPurchaseType.VEHICLE when Enum.TryParse(order.OrderedVehicle.Model,
                                                                             true,
                                                                             out VehicleModel vehicleModel):
                    characterCosts += await _vehicleModule.GetPointsPrice(vehicleModel.ToString());
                    break;
            }
        }

        if (characterCreatorData.HasPhone)
        {
            var phoneCatalogItem = await _itemCatalogService.GetByKey(ItemCatalogIds.PHONE);
            characterCosts += _southCentralPointsModule.GetPointsPrice(phoneCatalogItem.Price);
        }

        foreach (var item in characterCreatorData.CharacterModel.InventoryModel.Items)
        {
            if (ClothingModule.IsClothesOrProp(item.CatalogItemModelId))
            {
                continue;
            }

            var componentId = ClothingModule.GetComponentId(item.CatalogItemModelId);
            if (!componentId.HasValue)
            {
                continue;
            }

            var dbItem = await _itemCatalogService.GetByKey(item.CatalogItemModelId);
            characterCosts += (int)Math.Round(dbItem.Price * _gameOptions.MoneyToPointsExchangeRate);
        }

        return characterCosts;
    }
}