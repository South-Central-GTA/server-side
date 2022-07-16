using AltV.Net.Async;
using Microsoft.Extensions.Options;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.CommandSystem;
using Server.Core.Configuration;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Handlers.Company.PlayerVehicleWorkshop;

namespace Server.ChatCommands.Main;

public class RepairVehicleCommand : ISingletonScript
{
    private readonly CompanyGroupService _companyGroupService;
    private readonly DeliveryOptions _deliveryOptions;
    private readonly VehicleCatalogService _vehicleCatalogService;

    public RepairVehicleCommand(CompanyGroupService companyGroupService, IOptions<DeliveryOptions> deliveryOptions, VehicleCatalogService vehicleCatalogService)
    {
        _companyGroupService = companyGroupService;
        _vehicleCatalogService = vehicleCatalogService;
        _deliveryOptions = deliveryOptions.Value;
    }

    [Command("service", "In einem Fahrzeuginteraktionspunkt kannst du bei einem Unternehmen deren Service nutzen.")]
    public async void OnExecute(ServerPlayer player)
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
            var gender = player.CharacterModel.Gender == GenderType.MALE ? "der Fahrer" : "die Fahrerin";
            player.SendNotification($"Dein Charakter muss {gender} des Fahrzeug sein.", NotificationType.ERROR);
            return;
        }

        var vehicle = (ServerVehicle)player.Vehicle;
        if (vehicle is not { Exists: true } || vehicle.DbEntity == null)
        {
            player.SendNotification("Dieses Fahrzeug kannst du nicht reparieren.", NotificationType.ERROR);
            return;
        }
        
        var catalogVehicle = await _vehicleCatalogService.GetByKey(vehicle.DbEntity.Model.ToLower());
        if (catalogVehicle == null)
        {
            return;
        }
        
        var companyGroup = await _companyGroupService.GetByClosestVehicleInteractionPoint(player.Position);
        if (companyGroup == null)
        {
            player.SendNotification("Es befindet sich kein Fahrzeuginteraktionspunkt in der Nähe.", NotificationType.ERROR);
            return;
        }
        
        player.SetUniqueDimension();
        
        player.SetData("VEHICLE_SERVICE_DATA", new VehicleServiceData()
        {
            VehicleDbId = vehicle.DbEntity.Id,
        });
        
        player.SetData("VEHICLE_SERVICE_COMPANY_ID", companyGroup.Id);

        var fullHealth = vehicle.BodyHealth + vehicle.EngineHealth;
        var enginePercentage = fullHealth * 0.001;
        var percentage = (float)(enginePercentage * 0.5);
        
        player.EmitLocked("vehicleservice:start", new VehicleServiceInfoData()
        {
            ProductCount = companyGroup.Products,
            CurrentProductPrice = _deliveryOptions.ProductPrice,
            VehiclePrice = catalogVehicle.Price,
            VehicleModelName = catalogVehicle.Model.ToLower(),
            PrimaryColor = vehicle.PrimaryColor,
            SecondaryColor = vehicle.SecondaryColor,
            VehicleDamagePercentage = percentage
        });
        
        vehicle.Remove();
    }
}