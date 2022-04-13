using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Modules.Phone;

namespace Server.Handlers.Phone;

public class DenyCallHandler : ISingletonScript
{
    private readonly PhoneCallModule _phoneCallModule;

    public DenyCallHandler(PhoneCallModule phoneCallModule)
    {
        _phoneCallModule = phoneCallModule;

        AltAsync.OnClient<ServerPlayer>("phone:denycall", OnDenyCall);
    }

    private async void OnDenyCall(ServerPlayer player)
    {
        _phoneCallModule.DenyCall(player);
    }
}