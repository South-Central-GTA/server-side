using System.Linq;
using AltV.Net;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Banking;
using Server.Modules.Bank;

namespace Server.Handlers.Bank;

public class TransferBankHandler : ISingletonScript
{
    private readonly BankAccountService _bankAccountService;
    private readonly BankHistoryService _bankHistoryService;
    private readonly GroupService _groupService;
    
    private readonly BankModule _bankModule;

    public TransferBankHandler(
        BankAccountService bankAccountService, 
        BankHistoryService bankHistoryService, 
        GroupService groupService, BankModule bankModule)
    {
        _bankAccountService = bankAccountService;
        _bankHistoryService = bankHistoryService;
        _groupService = groupService;
        _bankModule = bankModule;

        AltAsync.OnClient<ServerPlayer, int, string, int, string>("bank:transfer", OnTransfer);
    }

    private async void OnTransfer(ServerPlayer player, int ownBankAccountId, string recieverBankAccountDetails,
                                  int value, string useOfPurpose = "")
    {
        if (!player.Exists)
        {
            return;
        }

        if (0 >= value)
        {
            player.SendNotification("Es muss eine positive Zahl welche größer ist als Null genutzt werden.",
                                    NotificationType.ERROR);
            return;
        }

        var bankAccount = await _bankAccountService.GetByKey(ownBankAccountId);
        if (bankAccount == null)
        {
            player.SendNotification("Das Bankkonto existiert nicht mehr.", NotificationType.ERROR);
            return;
        }

        var targetBankAccount = await _bankAccountService.GetByBankDetails(recieverBankAccountDetails);
        if (targetBankAccount == null)
        {
            player.SendNotification($"Das Bankkonto {recieverBankAccountDetails} existiert nicht.",
                                    NotificationType.ERROR);
            return;
        }

        if (!await _bankModule.HasPermission(player, bankAccount, BankingPermission.TRANSFER))
        {
            return;
        }

        if (bankAccount.Id == targetBankAccount.Id)
        {
            player.SendNotification("Dein Charakter kann keine Überweisung auf das selbe Konto tätigen.",
                                    NotificationType.ERROR);
            return;
        }

        if (bankAccount.Amount < value)
        {
            player.SendNotification(
                $"Es steht auf dem Bankkonto {bankAccount.BankDetails} nicht genügend Geld zur Verfügung.",
                NotificationType.ERROR);
            return;
        }

        bankAccount.Amount -= value;
        targetBankAccount.Amount += value;

        await _bankAccountService.Update(bankAccount);
        await _bankAccountService.Update(targetBankAccount);

        await _bankHistoryService.AddRange(new[]
        {
            new BankHistoryEntryModel
            {
                BankAccountModelId = bankAccount.Id,
                HistoryType = BankHistoryType.TRANSFER,
                Income = false,
                Amount = value,
                PurposeOfUse = useOfPurpose
            },
            new BankHistoryEntryModel
            {
                BankAccountModelId = targetBankAccount.Id,
                HistoryType = BankHistoryType.TRANSFER,
                Income = true,
                Amount = value,
                PurposeOfUse = useOfPurpose
            }
        });

        foreach (var targetPlayer
                 in bankAccount.CharacterAccesses.Select(characterAccess =>
                                                             Alt.GetAllPlayers()
                                                                .FindPlayerByCharacterId(characterAccess.CharacterModelId))
                               .Where(serverPlayer => serverPlayer != null))
        {
            await _bankModule.UpdateUi(targetPlayer);
        }

        foreach (var groupAccess in targetBankAccount.GroupRankAccess)
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

        player.SendNotification(
            $"Dein Charakter hat erfolgreich ${value} auf das Bankkonto {targetBankAccount.BankDetails} gesendet.",
            NotificationType.SUCCESS);
    }
}