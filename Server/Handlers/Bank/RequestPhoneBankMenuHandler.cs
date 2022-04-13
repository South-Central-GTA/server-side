using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Modules.Bank;

namespace Server.Handlers.Bank;

public class RequestPhoneBankMenuHandler : ISingletonScript
{
    private readonly BankModule _bankModule;

    public RequestPhoneBankMenuHandler(BankModule bankModule)
    {
        _bankModule = bankModule;

        AltAsync.OnClient<ServerPlayer>("phonebank:requestapp", OnRequestMenu);
    }

    private async void OnRequestMenu(ServerPlayer player)
    {
        await _bankModule.UpdateUi(player);
    }
}