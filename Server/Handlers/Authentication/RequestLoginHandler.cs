using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Modules.Authentication;
using Server.Modules.Discord;

namespace Server.Handlers.Authentication;

public class RequestLoginHandler : ISingletonScript
{
    private readonly AuthenticationModule _authenticationModule;
    private readonly AccountService _accountService;
    private readonly DiscordModule _discordModule;

    public RequestLoginHandler(
        AuthenticationModule authenticationModule,
        AccountService accountService,
        DiscordModule discordModule)
    {
        _authenticationModule = authenticationModule;
        _accountService = accountService;
        _discordModule = discordModule;

        AltAsync.OnClient<ServerPlayer, ulong, string>("auth:requestlogin", OnAuthRequestLogin);
    }

    private async void OnAuthRequestLogin(ServerPlayer player, ulong discordId, string token)
    {
        if (!player.Exists)
        {
            return;
        }

        player.DiscordId = discordId;

        await _discordModule.AuthenticatePlayer(player, token);

        if (await _accountService.Exists(a => a.SocialClubId == player.SocialClubId))
        {
            await _authenticationModule.SignIn(player);
        }
        else
        {
            await _authenticationModule.SignUp(player);
        }
    }
}