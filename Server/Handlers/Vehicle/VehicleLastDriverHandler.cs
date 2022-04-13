using System.Collections.Generic;
using AltV.Net;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Helper;
using Server.Modules.Narrator;

namespace Server.Handlers.Vehicle;

public class VehicleLastDriverHandler : ISingletonScript
{
    private readonly CharacterService _characterService;
    private readonly Serializer _serializer;

    private readonly NarratorModule _narratorModule;
    
    public VehicleLastDriverHandler(
        Serializer serializer,
        CharacterService characterService, 
        
        NarratorModule narratorModule)
    {
        _serializer = serializer;
        _characterService = characterService;
        
        _narratorModule = narratorModule;

        AltAsync.OnClient<ServerPlayer, int>("vehiclemenu:requestlastdrivers", OnRequestLastDrivers);
    }

    private async void OnRequestLastDrivers(ServerPlayer player, int vehicleDbId)
    {
        if (!player.Exists)
        {
            return;
        }

        var vehicle = Alt.GetAllVehicles().FindByDbId(vehicleDbId);
        if (vehicle is not { Exists: true } || vehicle.DbEntity == null)
        {
            player.SendNotification("Bei diesem Fahrzeug kannst du dir die letzten Fahrer nicht anzeigen.", NotificationType.ERROR);
            return;
        }

        var names = new List<string>();
        if (vehicle.DbEntity.LastDrivers.Count == 0)
        {
            _narratorModule.SendMessage(player, $"Das Fahrzeug hat noch keine letzten Fahrer.");
            return;
        }
        
        for (var index = vehicle.DbEntity.LastDrivers.Count - 1; index >= 0; index--)
        {
            var lastDriverData =
                _serializer.Deserialize<LastDriverData>(vehicle.DbEntity.LastDrivers[index]);

            var character = await _characterService.GetByKey(lastDriverData.CharacterId);
            if (character != null)
            {
                names.Add(character.Name + " am: " + lastDriverData.Date);
            }
        }

        _narratorModule.SendMessage(player, $"Die letzten Fahrer dieses Fahrzeuges waren {string.Join(", ", names)}.");
    }
}