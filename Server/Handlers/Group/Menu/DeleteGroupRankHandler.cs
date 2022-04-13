using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Modules.Group;

namespace Server.Handlers.Group;

public class DeleteGroupRankHandler : ISingletonScript
{
    private readonly GroupModule _groupModule;
    private readonly GroupService _groupService;

    public DeleteGroupRankHandler(
        GroupService groupService,
        GroupModule groupModule)
    {
        _groupService = groupService;

        _groupModule = groupModule;

        AltAsync.OnClient<ServerPlayer, int, int>("groupmenu:deleterank", OnDeleteRank);
    }

    private async void OnDeleteRank(ServerPlayer player, int groupId, int level)
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

        var success = await _groupModule.DeleteRank(group, level);
        if (!success)
        {
            player.SendNotification("Der Rang konnte nicht gelöscht werden, da er noch genutzt wird.", NotificationType.ERROR);
            return;
        }

        await _groupModule.UpdateUi(player);
    }
}