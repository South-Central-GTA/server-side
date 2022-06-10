using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Modules.Group;
using Server.Modules.Houses;

namespace Server.Handlers.Group;

public class DeleteGroupMenuHandler : ISingletonScript
{
    private readonly GroupModule _groupModule;
    private readonly GroupService _groupService;
    private readonly HouseModule _houseModule;
    private readonly HouseService _houseService;

    public DeleteGroupMenuHandler(GroupService groupService, HouseService houseService, GroupModule groupModule,
        HouseModule houseModule)
    {
        _groupService = groupService;
        _houseService = houseService;

        _groupModule = groupModule;
        _houseModule = houseModule;

        AltAsync.OnClient<ServerPlayer, int>("groupmenu:deletegroup", OnDeleteGroup);
    }

    private async void OnDeleteGroup(ServerPlayer player, int groupId)
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
            player.SendNotification("Dein Charakter ist nicht der Eigentümer der Gruppe.", NotificationType.ERROR);
            return;
        }

        var ownedGroupHouses = await _houseService.Where(h => h.GroupModelId == group.Id);

        await _houseModule.ResetOwners(ownedGroupHouses);

        await _groupModule.DeleteGroup(group);
        player.SendNotification("Du hast die Gruppe gelöscht.", NotificationType.SUCCESS);
    }
}