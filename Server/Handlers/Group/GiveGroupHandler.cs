using AltV.Net;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Modules.Group;

namespace Server.Handlers.Group;

public class GiveGroupHandler : ISingletonScript
{
    private readonly GroupMemberService _groupMemberService;
    private readonly GroupModule _groupModule;
    private readonly GroupService _groupService;

    public GiveGroupHandler(
        GroupService groupService,
        GroupMemberService groupMemberService,
        GroupModule groupModule)
    {
        _groupService = groupService;
        _groupMemberService = groupMemberService;
        _groupModule = groupModule;

        AltAsync.OnClient<ServerPlayer, int, int>("group:givegroup", OnGiveGroup);
    }

    private async void OnGiveGroup(ServerPlayer player, int groupId, int targetPlayerId)
    {
        if (!player.Exists)
        {
            return;
        }

        var newOwner = Alt.GetAllPlayers().GetPlayerById(player, targetPlayerId);
        if (newOwner == null || !newOwner.Exists)
        {
            player.SendNotification("Es wurde kein Spieler gefunden.", NotificationType.ERROR);
            return;
        }

        var group = await _groupService.GetByKey(groupId);
        if (group == null)
        {
            player.SendNotification("Es konnte keine Gruppe mehr gefunden werden.", NotificationType.ERROR);
            newOwner.SendNotification("Es konnte keine Gruppe mehr gefunden werden.", NotificationType.ERROR);
            return;
        }

        if (!_groupModule.IsOwner(player, group))
        {
            player.SendNotification("Dein Charakter ist nicht der Eigentümer des Unternehmens.",
                                    NotificationType.ERROR);
            newOwner.SendNotification("Der Gruppentransfer konnte nicht abgeschlossen werden.", NotificationType.ERROR);
            return;
        }

        var groupMember = await _groupMemberService.Find(m => m.GroupModelId == group.Id
                                                              && m.CharacterModelId == newOwner.CharacterModel.Id);
        if (groupMember == null)
        {
            player.SendNotification("Dein Charakter ist nicht in der Gruppe.", NotificationType.ERROR);
            newOwner.SendNotification("Du bist noch nicht in der Gruppe.", NotificationType.ERROR);
            return;
        }

        await _groupModule.GiveGroup(groupMember, group);

        await _groupModule.UpdateUi(player);

        player.SendNotification($"Gruppe wurde erfolgreich an {newOwner.CharacterModel.Name} überschrieben.",
                                NotificationType.SUCCESS);
        newOwner.SendNotification("Gruppe wurde erfolgreich an dich überschrieben.", NotificationType.SUCCESS);
    }
}