using System;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Helper;
using Server.Modules.Vehicles;

namespace Server.Handlers.Company.PlayerVehicleWorkshop;

public class RequestPurchaseHandler : ISingletonScript
{
    private readonly PlayerVehicleWorkshopModule _playerVehicleWorkshopModule;
    private readonly CompanyGroupService _companyGroupService;
    private readonly VehicleService _vehicleService;
    private readonly VehicleModule _vehicleModule;
    private readonly Serializer _serializer;
    private readonly TuningModule _tuningModule;
    private readonly PlayerVehicleWorkshopModule _vehicleWorkshopModule;
    private readonly VehicleCatalogService _vehicleCatalogService;

    public RequestPurchaseHandler(PlayerVehicleWorkshopModule playerVehicleWorkshopModule, CompanyGroupService companyGroupService, 
        VehicleService vehicleService, VehicleModule vehicleModule, Serializer serializer, TuningModule tuningModule, 
        PlayerVehicleWorkshopModule vehicleWorkshopModule, VehicleCatalogService vehicleCatalogService)
    {
        _playerVehicleWorkshopModule = playerVehicleWorkshopModule;
        _companyGroupService = companyGroupService;
        _vehicleService = vehicleService;
        _vehicleModule = vehicleModule;
        _serializer = serializer;
        _tuningModule = tuningModule;
        _vehicleWorkshopModule = vehicleWorkshopModule;
        _vehicleCatalogService = vehicleCatalogService;

        AltAsync.OnClient<ServerPlayer, string>("vehicleservice:requestpurchase", OnExecute);
    }

    private async void OnExecute(ServerPlayer player, string ordersJson)
    {
        try
        {
            if (!player.Exists)
            {
                return;
            }

            player.Dimension = 0;

            if (!player.GetData("VEHICLE_SERVICE_DATA", out VehicleServiceData vehicleServiceData))
            {
                return;
            }
            
            if (!player.GetData("VEHICLE_SERVICE_COMPANY_ID", out int companyId))
            {
                return;
            }

            var companyGroup = await _companyGroupService.GetByKey(companyId);
            if (companyGroup == null)
            {
                return;
            }
            
            var playerVehicleModel = await _vehicleService.GetByKey(vehicleServiceData.VehicleDbId);
            if (playerVehicleModel == null)
            {
                return;
            }

            var orders = _serializer.Deserialize<PlayerVehicleWorkshopOrder[]>(ordersJson);
            var catalogVehicle = await _vehicleCatalogService.GetByKey(playerVehicleModel.Model.ToLower());
            if (catalogVehicle == null)
            {
                return;
            }
            
            var products = _vehicleWorkshopModule.CalculateProducts(catalogVehicle.Price, orders);
            if (companyGroup.Products < products)
            {
                player.SendNotification("Unternehmen hat nicht genug Produkte.", NotificationType.ERROR);
                
                player.DeleteData("VEHICLE_SERVICE_DATA");
                player.DeleteData("VEHICLE_SERVICE_COMPANY_ID");

                await _vehicleModule.Create(playerVehicleModel);
                return;
            }
            
            var costs = _vehicleWorkshopModule.CalculatePrice(catalogVehicle.Price, orders);

            vehicleServiceData.Orders = orders;
            player.SetData("VEHICLE_SERVICE_DATA", vehicleServiceData);
            
            player.CreateDialog(new DialogData
            {
                Type = DialogType.TWO_BUTTON_DIALOG,
                Title = "Werkstatt",
                Description =
                    $"Du würdest für <b>${costs}</b> das Fahrzeug tunen, wie möchtest du es bezahlen?<br><p class='text-muted'>Du kannst mit dem Bargeld deines Charakters bezahlen oder per Banküberweisung.</p>",
                HasBankAccountSelection = true,
                FreezeGameControls = true,
                PrimaryButton = "Bargeld nutzen",
                PrimaryButtonServerEvent = "playervehicleworkshop:buywithcash",
                SecondaryButton = "Karte nutzen",
                SecondaryButtonServerEvent = "playervehicleworkshop:buywithbank",
                CloseButtonServerEvent = "playervehicleworkshop:revokebuy"
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}