using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Banking;
using Server.Database.Models.Group;

namespace Server.Modules.Bank;

public class BankModule : ITransientScript
{
    private readonly BankAccountCharacterAccessService _bankAccountCharacterAccessService;
    private readonly BankAccountService _bankAccountService;
    private readonly BankHistoryService _bankHistoryService;
    private readonly GroupService _groupService;
    private readonly ILogger<BankModule> _logger;

    public BankModule(ILogger<BankModule> logger, BankAccountService bankAccountService, GroupService groupService,
        BankAccountCharacterAccessService bankAccountCharacterAccessService, BankHistoryService bankHistoryService)
    {
        _logger = logger;
        _bankAccountService = bankAccountService;
        _groupService = groupService;
        _bankAccountCharacterAccessService = bankAccountCharacterAccessService;
        _bankHistoryService = bankHistoryService;
    }

    public async Task UpdateUi(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        var bankAccounts = await _bankAccountService.GetByCharacter(player.CharacterModel.Id);
        var groups = await _groupService.Where(g => g.Members.Any(m => m.CharacterModelId == player.CharacterModel.Id));

        foreach (var group in groups)
        {
            var member = group.Members.FirstOrDefault(m => m.CharacterModelId == player.CharacterModel.Id);
            if (member == null)
            {
                continue;
            }

            if (!member.Owner)
            {
                var rank = group.Ranks.Find(r => r.Level == member.RankLevel);
                if (rank == null || !rank.GroupPermission.HasFlag(GroupPermission.BANKING_WITHDRAW) &&
                    !rank.GroupPermission.HasFlag(GroupPermission.BANKING_DEPOSIT) &&
                    !rank.GroupPermission.HasFlag(GroupPermission.BANKING_SEE_HISTORY))
                {
                    continue;
                }
            }

            bankAccounts.Add(await _bankAccountService.GetByGroup(group.Id));
        }

        player.EmitGui("bank:updatebankaccounts", bankAccounts);
    }

    public async Task CreateBankAccount(ServerPlayer player)
    {
        await _bankAccountService.Add(new BankAccountModel
        {
            Type = OwnableAccountType.PRIVATE,
            BankDetails = await GetUniqueBankDetails(),
            CharacterAccesses =
                new List<BankAccountCharacterAccessModel>
                {
                    new()
                    {
                        CharacterModelId = player.CharacterModel.Id,
                        Permission = BankingPermission.NONE,
                        Owner = true
                    }
                },
            GroupRankAccess = new List<BankAccountGroupRankAccessModel>()
        });
    }

    public async Task CreateBankAccount(GroupModel groupModel)
    {
        await _bankAccountService.Add(new BankAccountModel
        {
            Type = OwnableAccountType.GROUP,
            BankDetails = await GetUniqueBankDetails(),
            GroupRankAccess =
                new List<BankAccountGroupRankAccessModel> { new() { GroupModelId = groupModel.Id, Owner = true } },
            CharacterAccesses = new List<BankAccountCharacterAccessModel>()
        });
    }

    /// <summary>
    /// </summary>
    /// <param name="bankAccountModel"></param>
    /// <param name="amount"></param>
    /// <param name="force">
    ///     Force should only be used when you want to grab money without check, like payday public garage
    ///     costs.
    /// </param>
    /// <param name="useOfPurpose">Use of purpose for this transaction</param>
    /// <returns></returns>
    public async Task<bool> Withdraw(BankAccountModel bankAccountModel, int amount, bool force = false,
        string useOfPurpose = "")
    {
        if (!force && bankAccountModel.Amount < amount)
        {
            return false;
        }

        bankAccountModel.Amount -= amount;
        await _bankAccountService.Update(bankAccountModel);

        await _bankHistoryService.Add(new BankHistoryEntryModel
        {
            BankAccountModelId = bankAccountModel.Id,
            HistoryType = BankHistoryType.TRANSFER,
            Income = false,
            Amount = amount,
            PurposeOfUse = useOfPurpose
        });

        return true;
    }

    public async Task Deposit(BankAccountModel bankAccountModel, int amount, string useOfPurpose = "")
    {
        bankAccountModel.Amount += amount;
        await _bankAccountService.Update(bankAccountModel);

        await _bankHistoryService.Add(new BankHistoryEntryModel
        {
            BankAccountModelId = bankAccountModel.Id,
            HistoryType = BankHistoryType.TRANSFER,
            Income = true,
            Amount = amount,
            PurposeOfUse = useOfPurpose
        });
    }

    public async Task<bool> HasBankAccount(ServerPlayer player)
    {
        var bankAccount = await _bankAccountService.GetByCharacter(player.CharacterModel.Id);
        return bankAccount.Any(b => b.Status == BankAccountState.CREATED);
    }

    public async Task<bool> HasPermission(ServerPlayer player, BankAccountModel bankAccountModel,
        BankingPermission bankingPermission)
    {
        var hasAccess = false;

        var bankAccountCharacterAccess =
            bankAccountModel.CharacterAccesses.FirstOrDefault(ca => ca.CharacterModelId == player.CharacterModel.Id);
        if (bankAccountCharacterAccess == null)
        {
            var groups = await _groupService.GetGroupsByCharacter(player.CharacterModel.Id);
            if (groups == null)
            {
                return hasAccess;
            }

            foreach (var group in groups)
            {
                var bankAccountGroupAccess =
                    bankAccountModel.GroupRankAccess.FirstOrDefault(ga => ga.GroupModelId == group.Id);
                if (bankAccountGroupAccess == null)
                {
                    continue;
                }

                var groupMember = group.Members.Find(m => m.CharacterModelId == player.CharacterModel.Id);
                if (groupMember == null)
                {
                    continue;
                }

                if (groupMember.Owner)
                {
                    hasAccess = true;
                }

                var groupPermission = bankingPermission switch
                {
                    BankingPermission.DEPOSIT => GroupPermission.BANKING_DEPOSIT,
                    BankingPermission.WITHDRAW => GroupPermission.BANKING_WITHDRAW,
                    BankingPermission.TRANSFER => GroupPermission.BANKING_TRANSFER,
                    BankingPermission.SEE_HISTORY => GroupPermission.BANKING_SEE_HISTORY,
                    _ => GroupPermission.NONE
                };

                var rank = group.Ranks.Find(r => r.Level == groupMember.RankLevel);
                if (rank != null && rank.GroupPermission.HasFlag(groupPermission))
                {
                    hasAccess = true;
                }
            }
        }
        else
        {
            if (bankAccountCharacterAccess.Permission.HasFlag(bankingPermission) || bankAccountCharacterAccess.Owner)
            {
                hasAccess = true;
            }
        }

        return hasAccess;
    }

    public async Task<string> GetUniqueBankDetails(string bankCode = "SA")
    {
        var rnd = new Random();
        var firstNumber = rnd.Next(100000000, 999999999);
        var secondNumber = rnd.Next(1000, 9999);
        long number = firstNumber + secondNumber;

        var allBankAccounts = await _bankAccountService.GetAll();

        while (allBankAccounts.Exists(p => p.BankDetails == number.ToString()))
        {
            firstNumber = rnd.Next(100000000, 999999999);
            secondNumber = rnd.Next(1000, 9999);
            number = firstNumber + secondNumber;
        }

        return $"{bankCode}-{number}";
    }

    public async Task<bool> AddPermission(int bankAccountId, int characterId, BankingPermission permission)
    {
        var characterAccess = await _bankAccountCharacterAccessService.Find(ca =>
            ca.BankAccountModelId == bankAccountId && ca.CharacterModelId == characterId);
        if (characterAccess == null)
        {
            return false;
        }

        characterAccess.Permission |= permission;

        await _bankAccountCharacterAccessService.Update(characterAccess);
        return true;
    }

    public async Task<bool> RemovePermission(int bankAccountId, int characterId, BankingPermission permission)
    {
        var characterAccess = await _bankAccountCharacterAccessService.Find(ca =>
            ca.BankAccountModelId == bankAccountId && ca.CharacterModelId == characterId);
        if (characterAccess == null)
        {
            return false;
        }

        characterAccess.Permission &= ~permission;

        await _bankAccountCharacterAccessService.Update(characterAccess);
        return true;
    }
}