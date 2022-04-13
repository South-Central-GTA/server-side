using AltV.Net;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.Bank;
using Server.Modules.Group;

namespace Server.Handlers.Group;

public class InviteGroupHandler : ISingletonScript
{
    private readonly BankAccountService _bankAccountService;

    private readonly BankModule _bankModule;
    private readonly GroupModule _groupModule;
    private readonly GroupService _groupService;

    public InviteGroupHandler(
        BankAccountService bankAccountService,
        GroupService groupService,
        BankModule bankModule,
        GroupModule groupModule)
    {
        _bankAccountService = bankAccountService;
        _groupService = groupService;
        _bankModule = bankModule;
        _groupModule = groupModule;

        AltAsync.OnClient<ServerPlayer, int, int, int>("group:inviteaccept", OnInviteAccept);
        AltAsync.OnClient<ServerPlayer, int, int, int>("group:invitedeny", OnInviteDeny);
    }

    private async void OnInviteAccept(ServerPlayer player, int bankAccountId, int groupId, int inviterId)
    {
        if (!player.Exists)
        {
            return;
        }

        var bankAccount = await _bankAccountService.GetByKey(bankAccountId);
        if (bankAccount == null)
        {
            player.SendNotification("Das Bankkonto existiert nicht mehr.", NotificationType.ERROR);
            return;
        }

        if (!await _bankModule.HasPermission(player, bankAccount, BankingPermission.DEPOSIT))
        {
            player.SendNotification("Dein Charakter hat keine Einzahlrechte auf dem Bankkonto.", NotificationType.ERROR);
            return;
        }

        var group = await _groupService.GetByKey(groupId);
        if (group == null)
        {
            player.SendNotification("Es wurde keine Gruppe mehr gefunden.", NotificationType.ERROR);
            return;
        }

        var inviter = Alt.GetAllPlayers().GetPlayerById(player, inviterId);
        if (inviter == null)
        {
            return;
        }

        if (await _groupModule.IsPlayerInGroupType(player, GroupType.COMPANY))
        {
            player.SendNotification("Dein Charakter ist schon in einem spielerbasierten Unternehmen und kann deswegen nicht beitreten.", NotificationType.ERROR);
            return;
        }

        if (await _groupModule.IsPlayerInGroupType(player, GroupType.FACTION))
        {
            player.SendNotification("Dein Charakter ist schon in einer Fraktion und kann deswegen nicht beitreten.", NotificationType.ERROR);
            return;
        }

        if (player.CharacterModel.JobModel != null)
        {
            player.SendNotification("Dein Charakter hat einen definierten Job und kann deswegen nicht beitreten.", NotificationType.ERROR);
            return;
        }

        await _groupModule.Invite(inviter, group, player);
    }

    private async void OnInviteDeny(ServerPlayer player, int bankAccountId, int groupId, int inviterId)
    {
        if (!player.Exists)
        {
            return;
        }

        var inviter = Alt.GetAllPlayers().GetPlayerById(player, inviterId);
        if (inviter == null)
        {
            return;
        }

        player.SendNotification("Du hast die Einladung abgelehnt.", NotificationType.INFO);
        inviter.SendNotification($"Der Charakter {player.CharacterModel.Name} hat die Anfrage das Unternehmen beizutreten abgelehnt.", NotificationType.WARNING);
    }
}