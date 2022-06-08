using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Modules.Bank;

namespace Server.Handlers.Bank;

public class AtmHandler : ISingletonScript
{
    private readonly BankModule _bankModule;

    public AtmHandler(BankModule bankModule)
    {
        _bankModule = bankModule;

        AltAsync.OnClient<ServerPlayer>("atm:requestopenmenu", OnRequestOpenMenu);
    }

    private async void OnRequestOpenMenu(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        await _bankModule.UpdateUi(player);

        player.EmitLocked("atm:openmenu");
    }
}