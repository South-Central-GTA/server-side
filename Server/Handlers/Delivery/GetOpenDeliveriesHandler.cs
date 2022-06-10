using System.Linq;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Group;
using Server.Modules.Delivery;

namespace Server.Handlers.Delivery;

public class GetOpenDeliveriesHandler : ISingletonScript
{
    private readonly DeliveryModule _deliveryModule;

    private readonly GroupService _groupService;

    public GetOpenDeliveriesHandler(GroupService groupService, DeliveryModule deliveryModule)
    {
        _groupService = groupService;
        _deliveryModule = deliveryModule;

        AltAsync.OnClient<ServerPlayer>("delivery:getopendeliveries", OnGetOpenDeliveries);
    }

    private async void OnGetOpenDeliveries(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        var groups = await _groupService.GetGroupsByCharacter(player.CharacterModel.Id);
        var group = groups?.FirstOrDefault(g => g.GroupType == GroupType.COMPANY);
        if (group == null)
        {
            return;
        }

        var companyGroup = (CompanyGroupModel)group;

        if (!companyGroup.LicensesFlags.HasFlag(LicensesFlags.GOODS_TRANSPORT))
        {
            return;
        }

        await _deliveryModule.UpdatePlayerOpenDeliveriesUi(player);
    }
}