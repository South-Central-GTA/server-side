using System.Linq;
using AltV.Net;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Inventory;
using Server.Modules.Bank;
using Server.Modules.Money;
using Server.Modules.Phone;

namespace Server.Handlers.Bank;

public class WithdrawBankHandler : ISingletonScript
{
    private readonly BankModule _bankModule;
    private readonly MoneyModule _moneyModule;
    private readonly PhoneModule _phoneModule;

    private readonly BankAccountService _bankAccountService;
    private readonly GroupService _groupService;
    private readonly RegistrationOfficeService _registrationOfficeService;

    public WithdrawBankHandler(
        BankModule bankModule,
        MoneyModule moneyModule,
        PhoneModule phoneModule,
        BankAccountService bankAccountService,
        GroupService groupService,
        RegistrationOfficeService registrationOfficeService)
    {
        _bankModule = bankModule;
        _moneyModule = moneyModule;
        _phoneModule = phoneModule;

        _bankAccountService = bankAccountService;
        _groupService = groupService;
        _registrationOfficeService = registrationOfficeService;

        AltAsync.OnClient<ServerPlayer, int, int>("bank:withdraw", OnWithdraw);
    }

    private async void OnWithdraw(ServerPlayer player, int bankAccountId, int value)
    {
        if (!player.Exists)
        {
            return;
        }

        var isRegistered = await _registrationOfficeService.IsRegistered(player.CharacterModel.Id);
        if (!isRegistered)
        {
            player.SendNotification("Dein Charakter ist nicht im Registration Office gemeldet.",
                                    NotificationType.ERROR);
            return;
        }

        var bankAccount = await _bankAccountService.GetByKey(bankAccountId);
        if (bankAccount == null)
        {
            player.SendNotification("Das Bankkonto existiert nicht mehr.", NotificationType.ERROR);
            return;
        }

        if (!await _bankModule.HasPermission(player, bankAccount, BankingPermission.WITHDRAW))
        {
            return;
        }

        if (bankAccount.Amount < value)
        {
            player.SendNotification(
                $"Es steht auf dem Bankkonto {bankAccount.BankDetails} nicht genügend Geld zur Verfügung.",
                NotificationType.ERROR);
            return;
        }

        if (!await _moneyModule.GiveMoney(player, value))
        {
            player.SendNotification(
                "Dein Charakter hat nicht genug Geld im Inventar und konnte das Geld nicht einzahlen.",
                NotificationType.ERROR);
            return;
        }

        await _bankModule.Withdraw(bankAccount, value, false, "Bargeldauszahlung");
        player.SendNotification("Dein Charakter hat erfolgreich Geld abgehoben.", NotificationType.SUCCESS);

        foreach (var targetPlayer in bankAccount.CharacterAccesses.Select(characterAccess =>
                                                                              Alt.GetAllPlayers()
                                                                                 .FindPlayerByCharacterId(
                                                                                     characterAccess.CharacterModelId))
                                                .Where(serverPlayer => serverPlayer != null))
        {
            await _bankModule.UpdateUi(targetPlayer);
        }

        foreach (var groupAccess in bankAccount.GroupRankAccess)
        {
            var group = await _groupService.GetByKey(groupAccess.GroupModelId);

            foreach (var member in group.Members)
            {
                if (member.Owner)
                {
                    var ownerPlayer = Alt.GetAllPlayers().FindPlayerByCharacterId(member.CharacterModelId);
                    if (ownerPlayer != null)
                    {
                        await _bankModule.UpdateUi(ownerPlayer);
                        return;
                    }
                }

                var rank = group.Ranks.Find(r => r.Level == member.RankLevel);
                if (rank == null || !rank.GroupPermission.HasFlag(GroupPermission.BANKING_WITHDRAW)
                    && !rank.GroupPermission.HasFlag(GroupPermission.BANKING_DEPOSIT)
                    && !rank.GroupPermission.HasFlag(GroupPermission.BANKING_SEE_HISTORY))
                {
                    continue;
                }

                var targetPlayer = Alt.GetAllPlayers().FindPlayerByCharacterId(member.CharacterModelId);
                if (targetPlayer != null)
                {
                    await _bankModule.UpdateUi(targetPlayer);
                }
            }
        }
    }
}