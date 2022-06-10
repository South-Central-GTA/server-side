using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Modules.Group;

namespace Server.Handlers.Group;

public class LeaveGroupMenuHandler : ISingletonScript
{
    private readonly GroupModule _groupModule;
    private readonly GroupService _groupService;

    public LeaveGroupMenuHandler(GroupService groupService, GroupModule groupModule)
    {
        _groupService = groupService;
        _groupModule = groupModule;

        AltAsync.OnClient<ServerPlayer, int>("groupmenu:leavegroup", OnLeaveGroup);
    }

    private async void OnLeaveGroup(ServerPlayer player, int groupId)
    {
        if (!player.Exists)
        {
            return;
        }

        var group = await _groupService.GetByKey(groupId);
        if (group == null)
        {
            return;
        }

        await _groupModule.Leave(player, group);

        player.SendNotification("Du hast die Gruppe verlassen.", NotificationType.SUCCESS);
    }
}