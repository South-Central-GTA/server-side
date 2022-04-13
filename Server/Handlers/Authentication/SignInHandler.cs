using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Modules.Authentication;

namespace Server.Handlers.Authentication;

public class SignInHandler : ISingletonScript
{
    private readonly AuthenticationModule _authenticationModule;
    
    public SignInHandler(AuthenticationModule authenticationModule)
    {
        _authenticationModule = authenticationModule;
        
        AltAsync.OnClient<ServerPlayer, string>("auth:signin", OnSignIn);
    }

    private async void OnSignIn(ServerPlayer player, string password)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!_authenticationModule.VerifyLoginTries(player))
        {
            return;
        }

        await _authenticationModule.SignIn(player, password);
    }
}