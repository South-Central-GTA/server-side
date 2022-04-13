using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Database.Enums;
using Server.Modules.Bank;
using Server.Modules.DefinedJob;
using Server.Modules.Group;

namespace Server.Handlers.DefinedJob;

public class SelectDefinedJobHandler : ISingletonScript
{
    private readonly BankModule _bankModule;
    private readonly DefinedJobModule _definedJobModule;
    private readonly GroupModule _groupModule;

    public SelectDefinedJobHandler(
        BankModule bankModule,
        GroupModule groupModule,
        DefinedJobModule definedJobModule)
    {
        _bankModule = bankModule;
        _groupModule = groupModule;
        _definedJobModule = definedJobModule;

        AltAsync.OnClient<ServerPlayer, int, int>("definedjob:select", OnSelectJob);
    }

    private async void OnSelectJob(ServerPlayer player, int jobId, int bankAccountId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!await _bankModule.HasBankAccount(player))
        {
            player.SendNotification("Dein Charakter braucht ein Bankkonto damit du ihm einen Job definieren kannst.", NotificationType.ERROR);
            return;
        }

        if (await _groupModule.IsPlayerInGroupType(player, GroupType.COMPANY))
        {
            player.SendNotification("Dein Charakter ist schon in einem spielerbasierten Unternehmen und kann deswegen keinen definierten Job haben.", NotificationType.ERROR);
            return;
        }

        if (await _groupModule.IsPlayerInGroupType(player, GroupType.FACTION))
        {
            player.SendNotification("Dein Charakter ist schon in einer Fraktion und kann deswegen keinen definierten Job haben.", NotificationType.ERROR);
            return;
        }

        await _definedJobModule.SelectJob(player, jobId, bankAccountId);
    }
}