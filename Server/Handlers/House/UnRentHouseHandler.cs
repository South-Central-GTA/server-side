using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.Houses;

namespace Server.Handlers.House;

public class UnRentHouseHandler : ISingletonScript
{
    private readonly HouseService _houseService;
    private readonly GroupService _groupService;

    private readonly HouseModule _houseModule;


    public UnRentHouseHandler(
        HouseService houseService,
        GroupService groupService,
        HouseModule houseModule)
    {
        _houseService = houseService;
        _groupService = groupService;

        _houseModule = houseModule;

        AltAsync.OnClient<ServerPlayer>("house:unrent", OnUnRentHouse);
    }

    private async void OnUnRentHouse(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        var house = await _houseService.GetByDistance(player.Position);
        if (house == null)
        {
            player.SendNotification("Es ist keine mietbare Immobilie in der Nähe.", NotificationType.ERROR);
            return;
        }

        if (!house.Rentable)
        {
            player.SendNotification("Dies ist eine gekaufte Immobilie du kannst hier kein Mietvertrag kündigen.",
                                    NotificationType.ERROR);
            return;
        }

        if (!house.CharacterModelId.HasValue || house.CharacterModelId.Value != player.CharacterModel.Id)
        {
            player.SendNotification("Dein Charakter ist nicht der Eigentümer der Immobilie.", NotificationType.ERROR);
            return;
        }

        if (house.GroupModelId.HasValue && house.HouseType != HouseType.COMPANY)
        {
            var group = await _groupService.GetByKey(house.GroupModelId.Value);
            if (group != null)
            {
                player.SendNotification(
                    $"Diese Immobilie hat das Unternehmen {group.Name} als Hauptsitz daher kann der Mietvertrag nicht gekündigt werden.",
                    NotificationType.ERROR);
            }

            return;
        }

        await _houseModule.ResetOwner(house);

        player.SendNotification("Erfolgreich Vertrag gekündigt.", NotificationType.SUCCESS);
    }
}