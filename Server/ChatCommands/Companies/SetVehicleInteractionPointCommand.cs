using System.Numerics;
using AltV.Net.Data;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.CommandSystem;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Enums.EntitySync;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.EntitySync;
using Server.Modules.Group;

namespace Server.ChatCommands.Companies;

internal class SetVehicleInteractionPointCommand : ISingletonScript
{
    private readonly CompanyGroupService _companyGroupService;
    private readonly HouseService _houseService;
    private readonly MarkerSyncModule _markerSyncModule;
    private readonly GroupModule _groupModule;

    public SetVehicleInteractionPointCommand(CompanyGroupService companyGroupService, MarkerSyncModule markerSyncModule, HouseService houseService, GroupModule groupModule)
    {
        _companyGroupService = companyGroupService;
        _markerSyncModule = markerSyncModule;
        _houseService = houseService;
        _groupModule = groupModule;
    }

    [Command("setvehiclepoint", "Setze die Position des Fahrzeuge Interaktionspunktes für das Unternehmen.")]
    public async void OnExecute(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        if (player.IsInVehicle)
        {
            player.SendNotification("Dein Charakter darf in keinem Fahrzeug sitzen.", NotificationType.ERROR);
            return;
        }

        var companyGroup = await _companyGroupService.GetByCharacter(player.CharacterModel.Id);
        if (companyGroup == null)
        {
            player.SendNotification("Dein Charakter ist in keinem Unternehmen.", NotificationType.ERROR);
            return;
        }

        if (!_groupModule.IsOwner(player, companyGroup))
        {
            player.SendNotification("Dies kann nur der Besitzer des Unternehmens.", NotificationType.ERROR);
            return;
        }
        
        if (!companyGroup.LicensesFlags.HasFlag(LicensesFlags.VEHICLE_WORKSHOP))
        {
            player.SendNotification("Das Unternehmen hat keine Werkstattlizenz der Marker würde kein Sinn machen.", NotificationType.ERROR);
            return;
        }

        var closeToBase = false;
        var house = await _houseService.Find(h => h.GroupModelId == companyGroup.Id);
        if (house != null)
        {
            closeToBase = player.Position.Distance(house.Position) <= 20;
        }

        if (!closeToBase)
        {
            player.SendNotification("Du musst im Umkreis von 20 Metern von deinem Unternehmenssitz sein.", NotificationType.ERROR);
            return;
        }

        companyGroup.VehicleInteractionPointX = player.Position.X;
        companyGroup.VehicleInteractionPointY = player.Position.Y;
        companyGroup.VehicleInteractionPointZ = player.Position.Z - 1;
        companyGroup.VehicleInteractionPointRoll = player.Rotation.Roll;
        companyGroup.VehicleInteractionPointPitch = player.Rotation.Pitch;
        companyGroup.VehicleInteractionPointYaw = player.Rotation.Yaw;

        if (companyGroup.MarkerId.HasValue)
        {
            var serverMarker = _markerSyncModule.Get(companyGroup.MarkerId.Value);
            if (serverMarker != null)
            {
                serverMarker.Position = new Vector3(companyGroup.VehicleInteractionPointX.Value, companyGroup.VehicleInteractionPointY.Value,
                    companyGroup.VehicleInteractionPointZ.Value);
            }
            
            player.SendNotification("Interaktionspunkt wurde erfolgreich verschoben.", NotificationType.SUCCESS);
        }
        else
        {
            var serverMarker = _markerSyncModule.Create(MarkerType.VERTICAL_CYLINDER,
                new Position(companyGroup.VehicleInteractionPointX.Value, companyGroup.VehicleInteractionPointY.Value, companyGroup.VehicleInteractionPointZ.Value),
                Vector3.Zero, Vector3.Zero, new Vector3(4f, 4f, 1f), new Rgba(255, 255, 255, 50), 0, false, 20,
                "~o~Fahrzeuginteraktionspunkt\n~w~Nutze /service");

            companyGroup.MarkerId = serverMarker.Id;
            
            player.SendNotification("Interaktionspunkt wurde erfolgreich erstellt.", NotificationType.SUCCESS);
        }
        
        
        await _companyGroupService.Update(companyGroup);
    }
}