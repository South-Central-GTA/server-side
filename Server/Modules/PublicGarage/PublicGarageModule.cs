using System.Collections.Generic;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
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
using Server.Database.Models.Vehicles;
using Server.Modules.Vehicles;

namespace Server.Modules.PublicGarage;

public class PublicGarageModule
    : ITransientScript
{
    private readonly BankAccountService _bankAccountService;

    private readonly ILogger<PublicGarageModule> _logger;
    private readonly PublicGarageEntryService _publicGarageEntryService;

    private readonly VehicleModule _vehicleModule;
    private readonly WorldLocationOptions _worldLocationOptions;

    public PublicGarageModule(
        ILogger<PublicGarageModule> logger,
        IOptions<WorldLocationOptions> worldLocationOptions,
        BankAccountService bankAccountService,
        PublicGarageEntryService publicGarageEntryService,
        VehicleModule vehicleModule)
    {
        _logger = logger;
        _worldLocationOptions = worldLocationOptions.Value;

        _bankAccountService = bankAccountService;
        _publicGarageEntryService = publicGarageEntryService;

        _vehicleModule = vehicleModule;
    }

    public Dictionary<int, IColShape> ColShapes { get; } = new();

    public PublicGarageData? FindGarage(int garageId)
    {
        return _worldLocationOptions.PublicGarages.Find(g => g.Id == garageId);
    }

    public async Task CreateCollShapes()
    {
        await AltAsync.Do(() =>
        {
            foreach (var garage in _worldLocationOptions.PublicGarages)
            {
                ColShapes.Add(garage.Id,
                              Alt.CreateColShapeSphere(
                                  new Position(garage.ParkingPointX, garage.ParkingPointY, garage.ParkingPointZ),
                                  4f));
            }
        });
    }

    public async Task Unpark(ServerPlayer player, PlayerVehicleModel vehicleModel, PublicGarageData publicGarageData, PublicGarageEntryModel publicGarageEntryModel)
    {
        var bankAccount = await _bankAccountService.GetByKey(publicGarageEntryModel.BankAccountId);
        if (bankAccount == null)
        {
            player.SendNotification("Das Bankkonto existiert nicht mehr.", NotificationType.ERROR);
            return;
        }

        if (bankAccount.Amount < 0)
        {
            player.SendNotification("Dein Fahrzeug ist gepfandet solang das angegebene Bankkonto deines Charakters im negativen Bereich liegt.", NotificationType.ERROR);
            return;
        }

        vehicleModel.VehicleState = VehicleState.SPAWNED;
        vehicleModel.LockState = LockState.CLOSED;

        vehicleModel.PositionX = publicGarageData.ParkingPointX;
        vehicleModel.PositionY = publicGarageData.ParkingPointY;
        vehicleModel.PositionZ = publicGarageData.ParkingPointZ;

        await _vehicleModule.Update(vehicleModel);
        await _vehicleModule.Create(vehicleModel);

        var genderString = player.CharacterModel.Gender == GenderType.MALE ? "sein" : "ihr";
        player.SendNotification($"Dein Charakter hat erfolgreich {genderString} Fahrzeug ausgeparkt.", NotificationType.SUCCESS);

        await _publicGarageEntryService.Remove(publicGarageEntryModel);
    }

    public async Task Park(ServerPlayer player, ServerVehicle vehicle, int garageId, int bankAccountId)
    {
        await _publicGarageEntryService.Add(new PublicGarageEntryModel { CharacterModelId = player.CharacterModel.Id, PlayerVehicleModelId = vehicle.DbEntity.Id, GarageId = garageId, BankAccountId = bankAccountId });

        vehicle.DbEntity.Position = Position.Zero;
        vehicle.DbEntity.Rotation = Rotation.Zero;
        vehicle.DbEntity.BodyHealth = await vehicle.GetBodyHealthAsync();
        vehicle.DbEntity.EngineHealth = await vehicle.GetEngineHealthAsync();
        vehicle.DbEntity.VehicleState = VehicleState.IN_GARAGE;

        await _vehicleModule.Update(vehicle.DbEntity);

        await vehicle.RemoveAsync();

        player.SendNotification("Dein Charakter hat das Fahrzeug erfolgreich eingeparkt.", NotificationType.SUCCESS);
    }
}