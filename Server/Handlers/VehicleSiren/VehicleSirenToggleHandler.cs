using AltV.Net.Async;
using Microsoft.Extensions.Logging;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;

namespace Server.Handlers.VehicleSiren;

public class VehicleSirenToggleHandler : ISingletonScript
{
    private readonly ILogger<VehicleSirenToggleHandler> _logger;
    private readonly VehicleCatalogService _vehicleCatalogService;

    public VehicleSirenToggleHandler(ILogger<VehicleSirenToggleHandler> logger,
        VehicleCatalogService vehicleCatalogService)
    {
        _logger = logger;
        _vehicleCatalogService = vehicleCatalogService;

        AltAsync.OnClient<ServerPlayer>("vehiclesiren:toggle", OnToggle);
    }

    private async void OnToggle(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.IsInVehicle)
        {
            return;
        }

        if (player.Vehicle is not ServerVehicle vehicle)
        {
            return;
        }

        if (vehicle.DbEntity == null)
        {
            return;
        }

        var catalogVehicle = await _vehicleCatalogService.GetByKey(vehicle.DbEntity.Model.ToLower());
        if (catalogVehicle == null)
        {
            return;
        }

        if (catalogVehicle.ClassId.ToUpper() != "EMERGENCY")
        {
            return;
        }

        if (vehicle.GetStreamSyncedMetaData("SIREN_MUTED", out bool muted))
        {
            vehicle.SetStreamSyncedMetaData("SIREN_MUTED", !muted);
        }
        else
        {
            vehicle.SetStreamSyncedMetaData("SIREN_MUTED", true);
        }
    }
}