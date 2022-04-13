using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.CommandSystem;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;

namespace Server.ChatCommands.Moderation;

public class DeleteDatabaseVehicleCommand : ISingletonScript
{
    private readonly VehicleService _vehicleService;

    public DeleteDatabaseVehicleCommand(VehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    [Command("ddv", "Zerstört das Datenbank Fahrzeug in welchem du sitzt.", Permission.ADMIN)]
    public async void OnDatabaseDestroyVehicle(ServerPlayer player)
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

        if (player.Vehicle is not ServerVehicle vehicle)
        {
            return;
        }

        if (vehicle.DbEntity == null)
        {
            player.SendNotification("Dies ist kein Datenbank Fahrzeug.", NotificationType.ERROR);
            return;
        }

        await _vehicleService.Remove(vehicle.DbEntity);
        await vehicle.RemoveAsync();

        player.SendNotification("Du hast das Datenbank Fahrzeug zerstört.", NotificationType.SUCCESS);
    }
}