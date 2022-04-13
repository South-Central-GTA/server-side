using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Modules.Bank;
using Server.Modules.DefinedJob;
using Server.Modules.Group;

namespace Server.Handlers.DefinedJob;

public class ChangeBankAccountDefinedJobHandler : ISingletonScript
{
    private readonly BankModule _bankModule;
    private readonly DefinedJobModule _definedJobModule;
    private readonly GroupModule _groupModule;

    public ChangeBankAccountDefinedJobHandler(
        BankModule bankModule,
        GroupModule groupModule,
        DefinedJobModule definedJobModule)
    {
        _bankModule = bankModule;
        _groupModule = groupModule;
        _definedJobModule = definedJobModule;

        AltAsync.OnClient<ServerPlayer, int>("definedjob:changebankaccount", OnChangeBankAccount);
    }

    private async void OnChangeBankAccount(ServerPlayer player, int bankAccountId)
    {
        await _definedJobModule.ChangeBankAccount(player, bankAccountId);
    }
}