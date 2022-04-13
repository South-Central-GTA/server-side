using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Modules.Phone;

namespace Server.Handlers.Phone;

public class HangupCallHandler : ISingletonScript
{
    private readonly PhoneCallModule _phoneCallModule;

    public HangupCallHandler(PhoneCallModule phoneCallModule)
    {
        _phoneCallModule = phoneCallModule;
        
        AltAsync.OnClient<ServerPlayer>("phone:hangup", OnHangup);
    }

    private async void OnHangup(ServerPlayer player)
    {
        _phoneCallModule.Hangup(player);
    }
}