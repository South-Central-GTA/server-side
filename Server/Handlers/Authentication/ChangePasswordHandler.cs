using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Modules.Authentication;

namespace Server.Handlers.Authentication;

public class ChangePasswordHandler : ISingletonScript
{
    private readonly AuthenticationModule _authenticationModule;

    public ChangePasswordHandler(AuthenticationModule authenticationModule)
    {
        _authenticationModule = authenticationModule;
        
        AltAsync.OnClient<ServerPlayer, string, string>("auth:changepassword", OnChangePassword);
    }

    private async void OnChangePassword(ServerPlayer player, string oldPassword, string newPassword)
    {
        await _authenticationModule.ChangePassword(player, oldPassword, newPassword);
    }
}
