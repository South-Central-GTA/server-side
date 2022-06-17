using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AltV.Net.Async;
using AltV.Net.Enums;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Helper;
using Server.Modules.Clothing;
using Server.Modules.DrivingSchool;

namespace Server.Handlers.Vehicle;

public class VehicleEnterHandler : ISingletonScript
{
    private readonly ClothingModule _clothingModule;
    private readonly DrivingSchoolModule _drivingSchoolModule;
    private readonly Serializer _serializer;

    private readonly VehicleCatalogService _vehicleCatalogService;
    private readonly VehicleService _vehicleService;

    public VehicleEnterHandler(Serializer serializer, DrivingSchoolModule drivingSchoolModule, ClothingModule clothingModule,
        VehicleCatalogService vehicleCatalogService, VehicleService vehicleService)
    {
        _serializer = serializer;

        _drivingSchoolModule = drivingSchoolModule;
        _clothingModule = clothingModule;

        _vehicleCatalogService = vehicleCatalogService;
        _vehicleService = vehicleService;

        AltAsync.OnPlayerEnteringVehicle += (vehicle, player, seat) =>
            OnPlayerEnteringVehicle(vehicle as ServerVehicle ?? throw new InvalidOperationException(), (ServerPlayer)player, seat);
        AltAsync.OnPlayerEnterVehicle += (vehicle, player, seat) =>
            OnPlayerEnterVehicle(vehicle as ServerVehicle ?? throw new InvalidOperationException(), (ServerPlayer)player, seat);
    }

    private async Task OnPlayerEnteringVehicle(ServerVehicle vehicle, ServerPlayer player, byte seat)
    {
        if (!player.Exists)
        {
            return;
        }

        var model = (VehicleModel)vehicle.Model;
        var catalogVehicle = await _vehicleCatalogService.GetByKey(model.ToString().ToLower());

        lock (player)
        {
            if (catalogVehicle == null)
            {
                if (player.IsAduty)
                {
                    player.SendNotification("Dieses Fahrzeug ist nicht in unserem Katalog definiert, bitte füge es hinzu.",
                        NotificationType.WARNING);
                    return;
                }

                player.SendNotification("Dieses Fahrzeug ist nicht in unserem Katalog definiert, wende dich bitte an einen Admin.",
                    NotificationType.ERROR);
                player.EmitLocked("player:cleartaskimmediately");
                return;
            }

            player.EmitLocked("vehicle:entering", vehicle);
        }

        if (vehicle.HasData("DRIVING_SCHOOL_CHARACTER_ID"))
        {
            vehicle.GetData("DRIVING_SCHOOL_CHARACTER_ID", out int id);
            if (id == player.CharacterModel.Id)
            {
                vehicle.LockState = VehicleLockState.Unlocked;
            }
        }
    }

    private async Task OnPlayerEnterVehicle(ServerVehicle vehicle, ServerPlayer player, byte seat)
    {
        if (!player.Exists)
        {
            return;
        }

        var model = (VehicleModel)vehicle.Model;
        var catalogVehicle = await _vehicleCatalogService.GetByKey(model.ToString().ToLower());

        if (catalogVehicle == null)
        {
            return;
        }

        if (catalogVehicle.ClassId == "CYCLE")
        {
            vehicle.ManualEngineControl = false;
            vehicle.EngineOn = true;
        }
        else
        {
            vehicle.ManualEngineControl = true;
        }

        if (seat == 1)
        {
            if (vehicle.DbEntity != null)
            {
                vehicle.DbEntity.LastDrivers.Add(_serializer.Serialize(new LastDriverData(player.CharacterModel.Id)));

                if (vehicle.DbEntity.LastDrivers.Count > 5)
                {
                    vehicle.DbEntity.LastDrivers.RemoveAt(0);
                }

                await _vehicleService.Update(vehicle.DbEntity);
            }
            else
            {
                if (vehicle.HasData("DRIVING_SCHOOL_CHARACTER_ID"))
                {
                    vehicle.GetData("DRIVING_SCHOOL_CHARACTER_ID", out int id);
                    if (id == player.CharacterModel.Id)
                    {
                        vehicle.LockState = VehicleLockState.Locked;
                        vehicle.ClearTimer("fail_exam");

                        if (!player.HasData("DRIVING_SCHOOL_VEH"))
                        {
                            player.SetData("DRIVING_SCHOOL_VEH", vehicle);
                            _drivingSchoolModule.RestartPlayerExam(player, vehicle);
                        }
                    }
                }
            }
        }

        _clothingModule.UpdateClothes(player);
    }
}