using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Modules.Authentication;

namespace Server.Handlers.Authentication;

public class SignUpHandler : ISingletonScript
{
    private readonly AuthenticationModule _authenticationModule;
    
    public SignUpHandler(AuthenticationModule authenticationModule)
    {
        _authenticationModule = authenticationModule;
        
        AltAsync.OnClient<ServerPlayer, string>("auth:signup", OnSignUp);
    }

    private async void OnSignUp(ServerPlayer player, string password)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!_authenticationModule.VerifyLoginTries(player))
        {
            return;
        }

        await _authenticationModule.SignUp(player, password);
    }
}