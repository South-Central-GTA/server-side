using System;
using System.Threading.Tasks;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Database.Enums;
using Server.Modules.Chat;
using Server.Modules.Vehicles;

namespace Server.Modules;

public class ShortingModule : ITransientScript
{
    private readonly Random _rand = new();
    private readonly VehicleModule _vehicleModule;
    private readonly ChatModule _chatModule;

    private readonly string[] _emotes =
    {
        "fummelt unter dem Lenkrad rum und wirkt sehr abgelenkt.", 
        "schaut sich um und ist unter dem Lenkrad beschäftigt.",
        "schraubt unter dem Lenkrad rum und schaut sich dabei um.",
        "schaut sich um und fummelt dann unter dem Lenkrad rum."
    };
    
    public ShortingModule(VehicleModule vehicleModule, ChatModule chatModule)
    {
        _vehicleModule = vehicleModule;
        _chatModule = chatModule;
    }

    public async Task ShortCircuitAsync(ServerPlayer player, ServerVehicle vehicle)
    {
        if (vehicle.EngineOn)
        {
            player.SendNotification("Der Motor des Fahrzeuges läuft schon.", NotificationType.INFO);
            return;
        }

        var returnVal = await _vehicleModule.CanToggleEngine(player, vehicle, !vehicle.EngineOn);
        switch (returnVal)
        {
            case EngineErrorType.NO_ENGINE:
                player.SendNotification("Das Fahrzeug hat kein Motor.", NotificationType.ERROR);
                return;

            case EngineErrorType.ENGINE_DAMAGED:
                player.SendNotification("Der Motor des Fahrzeuges ist beschädigt.", NotificationType.ERROR);
                return;

            case EngineErrorType.NO_FUEL:
                player.SendNotification("Der Motor hat kein Treibstoff.", NotificationType.ERROR);
                return;

            case EngineErrorType.NOT_SPAWNED:
                player.SendNotification("Das Fahrzeug ist nicht korrekt gespawnt.", NotificationType.ERROR);
                return;
        }
        
        await _chatModule.SendProxMessage(player, 30, ChatType.EMOTE, _emotes[_rand.Next(_emotes.Length)]);

        player.BlockGameControls(true);
        
        await Task.Delay(3000);        
        
        player.BlockGameControls(false);
        
        if (_rand.NextDouble() > 0.45) // 65% chance to fail
        {
            player.SendNotification("Fahrzeug kurzzuschließen fehlgeschlagen.", NotificationType.ERROR);
            return;
        }
        
        vehicle.EngineOn = true;

        player.SendNotification("Das Fahrzeug wurde kurzgeschlossen.", NotificationType.INFO);
    }
}