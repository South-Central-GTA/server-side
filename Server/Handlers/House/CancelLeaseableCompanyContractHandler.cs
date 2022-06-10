using System.Linq;
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

namespace Server.Handlers.House;

public class CancelLeaseableCompanyContractHandler : ISingletonScript
{
    private readonly GroupModule _groupModule;
    private readonly GroupService _groupService;
    private readonly HouseModule _houseModule;
    private readonly HouseService _houseService;
    private readonly PhoneModule _phoneModule;

    public CancelLeaseableCompanyContractHandler(GroupService groupService, HouseService houseService,
        GroupModule groupModule, PhoneModule phoneModule, HouseModule houseModule)
    {
        _groupService = groupService;
        _houseService = houseService;

        _groupModule = groupModule;
        _phoneModule = phoneModule;
        _houseModule = houseModule;

        AltAsync.OnClient<ServerPlayer, int, int, int>("leasecompany:cancelcontract", OnCancelContract);
    }

    private async void OnCancelContract(ServerPlayer player, int phoneId, int companyId, int leaseCompanyHouseId)
    {
        if (!player.Exists)
        {
            return;
        }

        var groups = await _groupService.GetGroupsByCharacter(player.CharacterModel.Id);
        var group = groups?.FirstOrDefault(g => g.GroupType == GroupType.COMPANY && g.Id == companyId);
        if (group == null)
        {
            return;
        }

        if (!_groupModule.IsOwner(player, group))
        {
            player.SendNotification("Dein Charakter ist nicht der Eigentümer des Unternehmens.",
                NotificationType.ERROR);
            return;
        }

        var house = await _houseService.GetByKey(leaseCompanyHouseId);
        if (house == null)
        {
            return;
        }

        await _houseModule.ResetOwner(house);

        await _houseModule.UpdateUi(player);

        await _phoneModule.SendNotification(phoneId, PhoneNotificationType.GOV,
            "Ihr Vertrag für den pachtbaren Unternehmenssitz wurde gekündigt.");
    }
}