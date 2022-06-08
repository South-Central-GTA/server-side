using System;
using System.Threading.Tasks;
using AltV.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Server.Core.Configuration;
using Server.Core.Extensions;
using Server.Core.ScheduledJobs;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.Bank;
using Server.Modules.Phone;

namespace Server.ScheduledJobs;

public class BankScheduledJob : ScheduledJob
{
    private readonly BankAccountService _bankAccountService;

    private readonly BankModule _bankModule;
    private readonly GroupService _groupService;
    private readonly ILogger<BankScheduledJob> _logger;
    private readonly PhoneModule _phoneModule;

    public BankScheduledJob(
        ILogger<BankScheduledJob> logger,
        IOptions<GameOptions> gameOptions,
        BankAccountService bankAccountService,
        GroupService groupService,
        BankModule bankModule,
        PhoneModule phoneModule)
        : base(TimeSpan.FromMinutes(gameOptions.Value.BankMinutesInterval))
    {
        _logger = logger;
        _bankAccountService = bankAccountService;
        _groupService = groupService;

        _bankModule = bankModule;
        _phoneModule = phoneModule;
    }

    public override async Task Action()
    {
        var bankAccounts = await _bankAccountService.Where(b => b.Status == BankAccountState.REQUESTED);

        // We have to toggle the state here and save it to the database because we need it to update it on ui side.
        bankAccounts.ForEach(b => b.Status = BankAccountState.CREATED);

        await _bankAccountService.UpdateRange(bankAccounts);

        foreach (var bankAccount in bankAccounts)
        {
            foreach (var access in bankAccount.CharacterAccesses)
            {
                var player = Alt.GetAllPlayers().FindPlayerByCharacterId(access.CharacterModelId);
                if (player != null)
                {
                    await _bankModule.UpdateUi(player);

                    var phone = await _phoneModule.GetByOwner(player.CharacterModel.Id);
                    if (phone != null)
                    {
                        await _phoneModule.SendNotification(phone.Id,
                                                            PhoneNotificationType.MAZE_BANK,
                                                            $"Ihr Bankkonto {bankAccount.BankDetails} wurde nun bei der Maze Bank freigeschaltet.");
                    }
                }
            }

            foreach (var groupAccess in bankAccount.GroupRankAccess)
            {
                var group = await _groupService.GetByKey(groupAccess.GroupModelId);

                foreach (var member in group.Members)
                {
                    if (member.Owner)
                    {
                        var owner = Alt.GetAllPlayers().FindPlayerByCharacterId(member.CharacterModelId);
                        if (owner != null)
                        {
                            await _bankModule.UpdateUi(owner);
                            continue;
                        }
                    }

                    var rank = group.Ranks.Find(r => r.Level == member.RankLevel);
                    if (rank == null || !rank.GroupPermission.HasFlag(GroupPermission.BANKING_WITHDRAW)
                        && !rank.GroupPermission.HasFlag(GroupPermission.BANKING_DEPOSIT)
                        && !rank.GroupPermission.HasFlag(GroupPermission.BANKING_SEE_HISTORY))
                    {
                        continue;
                    }

                    var player = Alt.GetAllPlayers().FindPlayerByCharacterId(member.CharacterModelId);
                    if (player != null)
                    {
                        await _bankModule.UpdateUi(player);
                    }
                }
            }
        }

        _logger.LogInformation($"{bankAccounts.Count} bank accounts got activated.");
        await Task.CompletedTask;
    }
}