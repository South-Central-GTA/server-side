using AltV.Net;
using AltV.Net.Async;
using Microsoft.Extensions.Options;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Configuration;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Models.Group;
using Server.Handlers.Company.PlayerVehicleWorkshop;

namespace Server.Modules.Vehicles;

public class PlayerVehicleWorkshopModule : ITransientScript
{
    private readonly VehicleCatalogService _vehicleCatalogService;
    private readonly DeliveryOptions _deliveryOptions;

    public PlayerVehicleWorkshopModule(
        VehicleCatalogService vehicleCatalogService, 
        IOptions<DeliveryOptions> deliveryOptions)
    {
        _vehicleCatalogService = vehicleCatalogService;
        _deliveryOptions = deliveryOptions.Value;
    }

    public void UpdateCompanyProductsCount(CompanyGroupModel companyGroup)
    {
        foreach (var target in Alt.GetAllPlayers()
                     .Where(p => p.GetData("VEHICLE_SERVICE_COMPANY_ID", out int companyId) && companyId == companyGroup.Id))
        {
            target.EmitLocked("vehicleservice:updateproducts", companyGroup.Products);
        }
    }

    public int CalculateProducts(int catalogVehiclePrice, PlayerVehicleWorkshopOrder[] orders)
    {
        var products = 0;
        
        foreach (var order in orders)
        {
            var price = GetTuningPartPrice(order.Type, order.Value);
            var moneyPrice = catalogVehiclePrice * 0.05f + price;
            var productCosts = (int)(moneyPrice / _deliveryOptions.ProductPrice * 0.3f);
            
            products += productCosts;
        }

        return products;
    }


    public int CalculatePrice(int catalogVehiclePrice, PlayerVehicleWorkshopOrder[] orders)
    {
        var price = 0;
        
        foreach (var order in orders)
        {
            var moneyPrice = (int)(catalogVehiclePrice * 0.05f + GetTuningPartPrice(order.Type, order.Value));
            price += moneyPrice;
        }

        return price;
    }

    private int GetTuningPartPrice(VehicleModType type, int level = 0)
    {
        var priceMap = new VehicleServicePriceTable();
        switch (type)
        {
            case VehicleModType.Spoilers:
                return priceMap.Spoilers;
            case VehicleModType.FrontBumper:
                return priceMap.FrontBumper;
            case VehicleModType.RearBumper:
                return priceMap.RearBumper;
            case VehicleModType.SideSkirt:
                return priceMap.SideSkirt;
            case VehicleModType.Exhaust:
                return priceMap.Exhaust;
            case VehicleModType.Frame:
                return priceMap.Frame;
            case VehicleModType.Grille:
                return priceMap.Grille;
            case VehicleModType.Hood:
                return priceMap.Hood;
            case VehicleModType.Fender:
                return priceMap.Fender;
            case VehicleModType.RightFender:
                return priceMap.RightFender;
            case VehicleModType.Roof:
                return priceMap.Roof;
            case VehicleModType.Engine:
                return priceMap.Engine[level];
            case VehicleModType.Brakes:
                return priceMap.Brakes[level];
            case VehicleModType.Transmission:
                return priceMap.Transmission;
            case VehicleModType.Horns:
                return priceMap.Horns;
            case VehicleModType.Suspension:
                return priceMap.Suspension;
            case VehicleModType.Armor:
                return priceMap.Armor;
            case VehicleModType.Turbo:
                return priceMap.Turbo;
            case VehicleModType.Xenon:
                return priceMap.Xenon;
            case VehicleModType.FrontWheels:
                return priceMap.FrontWheels;
            case VehicleModType.BackWheels:
                return priceMap.BackWheels;
            case VehicleModType.PlateHolder:
                return priceMap.PlateHolders;
            case VehicleModType.PlateVanity:
                return priceMap.PlateVanity;
            case VehicleModType.TrimDesign:
                return priceMap.TrimDesign;
            case VehicleModType.Ornaments:
                return priceMap.Ornaments;
            case VehicleModType.Dashboard:
                return priceMap.Dashboard;
            case VehicleModType.DialDesign:
                return priceMap.DialDesign;
            case VehicleModType.DoorSpeaker:
                return priceMap.DoorSpeaker;
            case VehicleModType.Seats:
                return priceMap.Seats;
            case VehicleModType.SteeringWheel:
                return priceMap.SteeringWheel;
            case VehicleModType.ShiftLever:
                return priceMap.ShiftLever;
            case VehicleModType.Plaques:
                return priceMap.Plaques;
            case VehicleModType.Speaker:
                return priceMap.Speaker;
            case VehicleModType.Trunk:
                return priceMap.Trunk;
            case VehicleModType.Hydraulics:
                return priceMap.Hydraulics;
            case VehicleModType.EngineBlock:
                return priceMap.EngineBlock;
            case VehicleModType.BoostOrAirFilter:
                return priceMap.BoostOrAirFilter;
            case VehicleModType.Struts:
                return priceMap.Struts;
            case VehicleModType.ArchCover:
                return priceMap.ArchCover;
            case VehicleModType.Aerials:
                return priceMap.Aerials;
            case VehicleModType.Trim:
                return priceMap.Trim;
            case VehicleModType.Tank:
                return priceMap.Tank;
            case VehicleModType.Windows:
                return priceMap.Windows;
            case VehicleModType.WindowTint:
                return priceMap.WindowTint;
            case VehicleModType.Livery:
                return priceMap.Livery;
            case VehicleModType.Plate:
                return priceMap.Plate;
            case VehicleModType.Colour1:
                return priceMap.Colour1;
            case VehicleModType.Colour2:
                return priceMap.Colour2;
        }

        return 0;
    }
}