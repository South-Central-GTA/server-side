using System;
using System.Linq;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Group;
using Server.Modules.Group;

namespace Server.Handlers.Company;

public class DeliveryVisibilityHandler : ISingletonScript
{
    private readonly GroupService _groupService;
    private readonly GroupModule _groupModule;

    public DeliveryVisibilityHandler(
        GroupService groupService,
        GroupModule groupModule)
    {
        _groupService = groupService;

        _groupModule = groupModule;

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