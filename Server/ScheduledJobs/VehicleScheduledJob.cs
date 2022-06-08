using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Server.Core.Callbacks;
using Server.Core.Configuration;
using Server.Core.Entities;
using Server.Core.ScheduledJobs;
using Server.DataAccessLayer.Services;
using Server.Database.Models.Vehicles;
using Server.Modules.Vehicles;
using VehicleModel = AltV.Net.Enums.VehicleModel;

namespace Server.ScheduledJobs;

public class VehicleScheduledJob : ScheduledJob
{
    private readonly VehicleOptions _vehicleOptions;
    private readonly ILogger<VehicleScheduledJob> _logger;
    private readonly VehicleCatalogService _vehicleCatalogService;

    private readonly VehicleModule _vehicleModule;

    public VehicleScheduledJob(
        ILogger<VehicleScheduledJob> logger,
        IOptions<VehicleOptions> vehicleOptions,
        VehicleCatalogService vehicleCatalogService,
        VehicleModule vehicleModule)
        : base(TimeSpan.FromSeconds(vehicleOptions.Value.VehicleFuelInterval))
    {
        _logger = logger;
        _vehicleOptions = vehicleOptions.Value;
        _vehicleCatalogService = vehicleCatalogService;
        _vehicleModule = vehicleModule;
    }

    public override async Task Action()
    {
        var vehicles = new List<ServerVehicle>();

        var callback = new AsyncFunctionCallback<IVehicle>(async vehicle =>
        {
            if (!vehicle.EngineOn || !vehicle.Exists)
            {
                return;
            }


            if (!vehicle.GetSyncedMetaData("ID", out int vehId))
            {
                return;
            }

            var modelName = ((VehicleModel)vehicle.Model).ToString().ToLower();
            var catalogVehicle = await _vehicleCatalogService.GetByKey(modelName);
            if (catalogVehicle == null)
            {
                return;
            }

            var serverVehicle = (ServerVehicle)vehicle;
            var speed = serverVehicle.Velocity.GetMagnitude() * 3.6f;

            await HandleFuel(serverVehicle, catalogVehicle, speed);
            await HandleKilometre(serverVehicle);

            vehicles.Add(serverVehicle);
        });

        await Alt.ForEachVehicles(callback);
        await _vehicleModule.SaveRange(vehicles);

        await Task.CompletedTask;
    }

    private async Task HandleFuel(ServerVehicle vehicle, CatalogVehicleModel catalogVehicleModel, float speed)
    {
        var classId = catalogVehicleModel.ClassId.ToLower();
        if (classId == "cycle")
        {
            return;
        }

        var fuelCosts = _vehicleOptions.VehicleFuelDefaultReduction;
        float modifier = 1;

        if (classId is "super" or "helicopter" or "plane")
        {
            modifier = 1.2f;
        }
        else if (classId is "muscle" or "suv" or "sport" or "sports_classic")
        {
            modifier = 1.1f;
        }
        else if (classId is "coupe")
        {
            modifier = 0.9f;
        }
        else if (classId is "compact")
        {
            modifier = 0.7f;
        }

        // More fuel cost because damaged engine.
        if (vehicle.EngineHealth <= _vehicleOptions.EngineDamageUntilMoreFuelCosts)
        {
            var additionalModifier = (1000 - vehicle.EngineHealth) * 0.01f;
            modifier += additionalModifier;
        }

        // More fuel cost because vehicle gets old.
        if (vehicle.DrivenKilometre >= _vehicleOptions.DrivenKilometerUntilMorFuelCosts)
        {
            modifier += 0.5f;
        }

        fuelCosts += speed * _vehicleOptions.VehicleFuelVelocityMultiplier * modifier;

        vehicle.Fuel -= fuelCosts;

        vehicle.SetSyncedMetaData("FUEL", vehicle.Fuel);

        if (vehicle.Fuel <= 0)
        {
            vehicle.EngineOn = false;
            vehicle.Fuel = 0;

            if (vehicle.Driver is ServerPlayer { IsAduty: true })
            {
                return;
            }

            await vehicle.SetEngineOnAsync(false);
        }
    }

    private async Task HandleKilometre(ServerVehicle vehicle)
    {
        vehicle.LastCheckPosition ??= vehicle.Position;
        var distance = vehicle.Position.Distance(vehicle.LastCheckPosition.Value);

        vehicle.DrivenKilometre += distance / 1000;

        vehicle.LastCheckPosition = vehicle.Position;

        await vehicle.SetSyncedMetaDataAsync("DRIVEN_KILOMETRE", vehicle.DrivenKilometre);
    }
}