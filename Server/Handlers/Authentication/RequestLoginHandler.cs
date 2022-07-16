using System;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Modules.Authentication;
using Server.Modules.Discord;

namespace Server.Handlers.Authentication;

public class RequestLoginHandler : ISingletonScript
{
    private readonly AccountService _accountService;
    private readonly AuthenticationModule _authenticationModule;
    private readonly DiscordModule _discordModule;

    public RequestLoginHandler(AuthenticationModule authenticationModule, AccountService accountService,
        DiscordModule discordModule)
    {
        _authenticationModule = authenticationModule;
        _accountService = accountService;
        _discordModule = discordModule;

        AltAsync.OnClient<ServerPlayer, ulong, string>("auth:requestlogin", OnAuthRequestLogin);
    }

    private async void OnAuthRequestLogin(ServerPlayer player, ulong discordId, string token)
    {
        try
        {
            if (!player.Exists)
            {
                return;
            }

            player.DiscordId = discordId;

            var discordUser = await _discordModule.Authenticate(token);

            if (!discordUser.HasValue)
            {
                player.Kick("Leider konnten wir dich nicht authentifizieren.");
                return;
            }

            if (await _accountService.Exists(a => a.SocialClubId == player.SocialClubId))
            {
                await _authenticationModule.SignIn(player, discordUser.Value);
            }
            else
            {
                await _authenticationModule.SignUp(player, discordUser.Value);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}