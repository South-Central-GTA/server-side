using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Modules.Phone;

namespace Server.Handlers.Phone;

public class AcceptCallHandler : ISingletonScript
{
    private readonly PhoneCallModule _phoneCallModule;

    public AcceptCallHandler(PhoneCallModule phoneCallModule)
    {
        _phoneCallModule = phoneCallModule;

        AltAsync.OnClient<ServerPlayer, int>("phone:acceptcall", OnAcceptCall);
    }

    private async void OnAcceptCall(ServerPlayer player, int phoneId)
    {
        await _phoneCallModule.AcceptCallAsync(player, phoneId);
    }
}