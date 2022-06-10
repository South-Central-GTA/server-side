using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Modules.Group;

namespace Server.Handlers.Group;

public class ChangeGroupRankNameHandler : ISingletonScript
{
    private readonly GroupModule _groupModule;
    private readonly GroupService _groupService;

    public ChangeGroupRankNameHandler(GroupService groupService, GroupModule groupModule)
    {
        _groupService = groupService;
        _groupModule = groupModule;

        AltAsync.OnClient<ServerPlayer, int, int, string>("groupmenu:changerankname", OnChangeRankName);
    }

    private async void OnChangeRankName(ServerPlayer player, int groupId, int rankLevel, string rankName)
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

        if (!_groupModule.IsOwner(player, group))
        {
            player.SendNotification("Dein Charakter muss Eigentümer von der Gruppe sein:", NotificationType.ERROR);
            return;
        }

        await _groupModule.RenameRank(groupId, rankLevel, rankName);
        await _groupModule.UpdateUi(player);
    }
}