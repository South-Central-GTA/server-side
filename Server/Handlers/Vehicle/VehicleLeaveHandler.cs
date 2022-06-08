using System;
using System.Threading.Tasks;
using System.Timers;
using AltV.Net;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Modules.DrivingSchool;
using Server.Modules.Vehicles;

namespace Server.Handlers.Vehicle
{
    public class VehicleLeaveHandler
        : ISingletonScript
    {
        private readonly DrivingSchoolModule _drivingSchoolModule;
        private readonly VehicleModule _vehicleModule;

        public VehicleLeaveHandler(
            VehicleModule vehicleModule,
            DrivingSchoolModule drivingSchoolModule)
        {
            _vehicleModule = vehicleModule;
            _drivingSchoolModule = drivingSchoolModule;

            AltAsync.OnPlayerLeaveVehicle += (vehicle, player, seat) =>
                OnPlayerLeaveVehicle(vehicle as ServerVehicle ?? throw new InvalidOperationException(),
                                     (ServerPlayer)player,
                                     seat);
        }

        private async Task OnPlayerLeaveVehicle(ServerVehicle vehicle, ServerPlayer player, byte seat)
        {
            if (!player.Exists)
            {
                return;
            }

            // if (vehicle.DbEntity != null)
            // {
            //     vehicle.DbEntity.Position = vehicle.Position;
            //     vehicle.DbEntity.Rotation = vehicle.Rotation;
            //
            //     await _vehicleModule.Update(vehicle.DbEntity);
            // }
            //
            // if (vehicle.Attached is ServerVehicle { DbEntity: { } } attachedVehicle)
            // {
            //     attachedVehicle.DbEntity.Position = vehicle.Position;
            //     attachedVehicle.DbEntity.Rotation = vehicle.Rotation;
            //
            //     await _vehicleModule.Update(attachedVehicle.DbEntity);
            // }

            if (vehicle.DbEntity == null)
            {
                if (vehicle.HasData("DRIVING_SCHOOL_CHARACTER_ID"))
                {
                    vehicle.GetData("DRIVING_SCHOOL_CHARACTER_ID", out int id);
                    if (id == player.CharacterModel.Id)
                    {
                        vehicle.CreateTimer("fail_exam",
                                            async (object sender, ElapsedEventArgs e) =>
                                            {
                                                var examPlayer = Alt.GetAllPlayers().FindPlayerByCharacterId(id);
                                                if (examPlayer != null)
                                                {
                                                    await _drivingSchoolModule.StopPlayerExam(examPlayer, false);
                                                    player.SendNotification(
                                                        "Der Fahrlehrer wollte nicht länger warten und ist zurück gefahren.",
                                                        NotificationType.ERROR);
                                                }
                                                else
                                                {
                                                    await _drivingSchoolModule.StopVehicleExam(vehicle);
                                                }
                                            },
                                            1000 * 60 * 2);
                    }
                }
            }

            player.UpdateClothes();
        }
    }
}