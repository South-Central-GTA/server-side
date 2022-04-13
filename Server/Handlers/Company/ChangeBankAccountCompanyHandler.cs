using System.Linq;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.Bank;

namespace Server.Handlers.Company;

public class ChangeBankAccountCompanyHandler : ISingletonScript
{
    private readonly BankAccountService _bankAccountService;
    private readonly GroupMemberService _groupMemberService;
    private readonly GroupService _groupService;
    private readonly BankModule _bankModule;

    public ChangeBankAccountCompanyHandler(
        GroupService groupService,
        BankAccountService bankAccountService,
        GroupMemberService groupMemberService,
        BankModule bankModule)
    {
        _groupService = groupService;
        _bankAccountService = bankAccountService;
        _groupMemberService = groupMemberService;

        _bankModule = bankModule;

        AltAsync.OnClient<ServerPlayer, int, int>("company:changebankaccount", OnChangeBankAccount);
    }

    private async void OnChangeBankAccount(ServerPlayer player, int companyId, int bankAccountId)
    {
        var groups = await _groupService.GetGroupsByCharacter(player.CharacterModel.Id);
        var group = groups?.FirstOrDefault(g => g.GroupType == GroupType.COMPANY && g.Id == companyId);
        if (group == null)
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
            player.SendNotification("Dein Charakter hat nicht genügend Berechtigungen mehr um dieses Bankkonto zu nutzen.", NotificationType.ERROR);
            return;
        }

        var member = group.Members.FirstOrDefault(gm => gm.CharacterModelId == player.CharacterModel.Id);
        if (member != null)
        {
            member.BankAccountId = bankAccountId;

            await _groupMemberService.Update(member);
        }

        player.SendNotification("Bankkonto wurde erfolgreich geändert.", NotificationType.SUCCESS);
    }
}