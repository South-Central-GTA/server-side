using System;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.DrivingSchool;

namespace Server.Handlers.Vehicle;

public class VehicleDestroyHandler : ISingletonScript
{
    private readonly DrivingSchoolModule _drivingSchoolModule;

    private readonly VehicleService _vehicleService;

    public VehicleDestroyHandler(DrivingSchoolModule drivingSchoolModule, VehicleService vehicleService)
    {
        _drivingSchoolModule = drivingSchoolModule;

        _vehicleService = vehicleService;

        AltAsync.OnVehicleDestroy += vehicle =>
            OnVehicleDestroy(vehicle as ServerVehicle ?? throw new InvalidOperationException());
    }

    private async Task OnVehicleDestroy(ServerVehicle vehicle)
    {
        if (vehicle.DbEntity == null)
        {
            if (vehicle.HasData("DRIVING_SCHOOL_CHARACTER_ID"))
            {
                vehicle.GetData("DRIVING_SCHOOL_CHARACTER_ID", out int id);
                var examPlayer = Alt.GetAllPlayers().FindPlayerByCharacterId(id);
                if (examPlayer != null)
                {
                    await _drivingSchoolModule.StopPlayerExam(examPlayer, false);
                }
                else
                {
                    await _drivingSchoolModule.StopVehicleExam(vehicle);
                }
            }

            return;
        }

        vehicle.DbEntity.VehicleState = VehicleState.DESTROYED;
        await _vehicleService.Update(vehicle.DbEntity);

        if (vehicle.DbEntity.CharacterModelId.HasValue)
        {
            var player = Alt.GetAllPlayers().FindPlayerByCharacterId(vehicle.DbEntity.CharacterModelId.Value);
            player?.SendNotification(
                "Eines deiner Fahrzeug wurde zerstört, spiele die Reperatur aus! " +
                "Du kannst es bei einer öffentlichen Garage neuspawnen.", NotificationType.WARNING);
        }
    }
}