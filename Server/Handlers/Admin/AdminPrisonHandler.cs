using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Modules.Admin;

namespace Server.Handlers.Admin;

public class AdminPrisonHandler : ISingletonScript
{
    private readonly AdminPrisonModule _adminPrisonModule;
    private readonly AccountService _accountService;
    
    public AdminPrisonHandler(
        AccountService accountService, 
        AdminPrisonModule adminPrisonModule)
    {
        _accountService = accountService;
        _adminPrisonModule = adminPrisonModule;
        
        AltAsync.OnClient<ServerPlayer>("adminprison:requestnextcheckpoint", OnRequestNextCheckpoint);
    }

    private async void OnRequestNextCheckpoint(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        player.AccountModel.AdminCheckpoints--;
        await _accountService.Update(player.AccountModel);

        if (player.AccountModel.AdminCheckpoints <= 0)
        {
            await _adminPrisonModule.ClearPlayerFromPrison(player, null);
            return;
        }

        player.EmitLocked("adminprison:sendnextcheckpoint", player.AccountModel.AdminCheckpoints);
    }
}