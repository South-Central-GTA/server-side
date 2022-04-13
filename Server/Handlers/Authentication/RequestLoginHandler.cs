using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Modules.Authentication;

namespace Server.Handlers.Authentication;

public class RequestLoginHandler : ISingletonScript
{
    private readonly AuthenticationModule _authenticationModule;
    private readonly AccountService _accountService;

    public RequestLoginHandler(
        AuthenticationModule authenticationModule, 
        AccountService accountService)
    {
        _authenticationModule = authenticationModule;
        _accountService = accountService;
        
        AltAsync.OnClient<ServerPlayer, ulong>("auth:requestlogin", OnAuthRequestLogin);
    }

    private async void OnAuthRequestLogin(ServerPlayer player, ulong discordId)
    {
        if (!player.Exists)
        {
            return;
        }

        player.DiscordId = discordId;

        if (await _accountService.Exists(a => a.SocialClubId == player.SocialClubId))
        {
            var account = await _accountService.GetByKey(player.SocialClubId);
            if (account == null)
            {
                return;
            }
            
            if (player.Ip == account.LastIp
                && player.HardwareIdHash == account.HardwareIdHash
                && player.HardwareIdExHash == account.HardwareIdExHash)
            {
                await _authenticationModule.ContinueLoginProcess(player, account);
            }
            else
            {
                player.EmitLocked("auth:showlogin");
            }
        }
        else
        {
            player.EmitLocked("auth:showsignup");
        }

        // This will be the players first login try.
        player.LoginTrys++;
    }
}