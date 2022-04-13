using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Modules.Group;

namespace Server.Handlers.Group;

public class CreateGroupRankHandler : ISingletonScript
{
    private readonly GroupModule _groupModule;
    private readonly GroupService _groupService;

    public CreateGroupRankHandler(
        GroupService groupService,
        GroupModule groupModule)
    {
        _groupService = groupService;
        _groupModule = groupModule;

        AltAsync.OnClient<ServerPlayer, int>("groupmenu:createrank", OnCreateRank);
    }

    private async void OnCreateRank(ServerPlayer player, int groupId)
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

        if (group.Ranks.Count >= group.MaxRanks)
        {
            player.SendNotification($"Deine Gruppe kann nicht mehr als {group.MaxRanks} Ränge haben.", NotificationType.ERROR);
            return;
        }

        if (!_groupModule.IsOwner(player, group))
        {
            player.SendNotification("Dein Charakter muss Eigentümer von der Gruppe sein.", NotificationType.ERROR);
            return;
        }

        await _groupModule.CreateRank(group, "Member");

        await _groupModule.UpdateUi(player);
    }
}