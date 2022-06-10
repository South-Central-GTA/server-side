using System.Linq;
using AltV.Net;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.Group;
using Server.Modules.Houses;
using Server.Modules.Phone;

namespace Server.Handlers.Company;

public class ChangeCompanyBuildingHandler : ISingletonScript
{
    private readonly GroupModule _groupModule;
    private readonly GroupService _groupService;
    private readonly HouseModule _houseModule;
    private readonly HouseService _houseService;
    private readonly PhoneModule _phoneModule;
    private readonly RegistrationOfficeService _registrationOfficeService;

    public ChangeCompanyBuildingHandler(GroupService groupService, HouseService houseService,
        RegistrationOfficeService registrationOfficeService, GroupModule groupModule, HouseModule houseModule,
        PhoneModule phoneModule)
    {
        _groupService = groupService;
        _houseService = houseService;
        _registrationOfficeService = registrationOfficeService;

        _groupModule = groupModule;
        _houseModule = houseModule;
        _phoneModule = phoneModule;

        AltAsync.OnClient<ServerPlayer, int, int>("company:changecompanybuilding", OnChangeCompanyHouse);
    }

    private async void OnChangeCompanyHouse(ServerPlayer player, int companyId, int houseId)
    {
        var groups = await _groupService.GetGroupsByCharacter(player.CharacterModel.Id);
        var group = groups?.FirstOrDefault(g => g.GroupType == GroupType.COMPANY && g.Id == companyId);

        var member = group?.Members.FirstOrDefault(m => m.CharacterModelId == player.CharacterModel.Id && m.Owner);
        if (member == null)
        {
            return;
        }

        var isRegistered = await _registrationOfficeService.IsRegistered(player.CharacterModel.Id);
        if (!isRegistered)
        {
            player.SendNotification("Dein Charakter ist nicht im Registration Office gemeldet.",
                NotificationType.ERROR);
            return;
        }

        var newHouse = await _houseService.GetByKey(houseId);
        if (newHouse == null)
        {
            return;
        }

        if (newHouse.HouseType != HouseType.HOUSE)
        {
            player.SendNotification("Dieses Gebäude kann nicht als Hauptsitz des Unternehmens gesetzt werden.",
                NotificationType.ERROR);
            return;
        }

        var oldHouse = await _houseService.Find(h => h.GroupModelId == group.Id);
        oldHouse.CharacterModelId = player.CharacterModel.Id;
        oldHouse.GroupModelId = null;
        await _houseService.Update(oldHouse);

        newHouse.GroupModelId = group.Id;
        await _houseService.Update(newHouse);

        foreach (var target in group.Members.Select(
                     m => Alt.GetAllPlayers().FindPlayerByCharacterId(m.CharacterModelId)))
        {
            if (target == null)
            {
                continue;
            }

            if (!target.Exists)
            {
                continue;
            }

            await _houseModule.UpdateUi(target);
        }

        await _houseModule.UpdateUi(player);

        await _groupModule.UpdateGroupUi(group);
    }
}