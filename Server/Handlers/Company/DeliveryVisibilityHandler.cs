using System;
using System.Linq;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Group;
using Server.Modules.Group;
using Server.Modules.Phone;

namespace Server.Handlers.Company;

public class DeliveryVisibilityHandler : ISingletonScript
{
    private readonly GroupModule _groupModule;
    private readonly GroupService _groupService;
    private readonly PhoneModule _phoneModule;
    private readonly RegistrationOfficeService _registrationOfficeService;

    public DeliveryVisibilityHandler(GroupService groupService, RegistrationOfficeService registrationOfficeService,
        GroupModule groupModule, PhoneModule phoneModule)
    {
        _groupService = groupService;
        _registrationOfficeService = registrationOfficeService;

        _groupModule = groupModule;
        _phoneModule = phoneModule;

        AltAsync.OnClient<ServerPlayer, int>("company:setdeliveryvisibility", OnSetDeliveryVisibility);
    }

    private async void OnSetDeliveryVisibility(ServerPlayer player, int companyId)
    {
        var groups = await _groupService.GetGroupsByCharacter(player.CharacterModel.Id);
        var group = groups?.FirstOrDefault(g => g.GroupType == GroupType.COMPANY && g.Id == companyId);
        if (group == null)
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

        var companyGroup = (CompanyGroupModel)group;

        switch (companyGroup.DeliveryVisibilityStatus)
        {
            case VisiblityState.PRIVATE:
                companyGroup.DeliveryVisibilityStatus = VisiblityState.PUBLIC;
                break;
            case VisiblityState.PUBLIC:
                companyGroup.DeliveryVisibilityStatus = VisiblityState.PRIVATE;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        await _groupService.Update(companyGroup);

        await _groupModule.UpdateGroupUi(group);
    }
}