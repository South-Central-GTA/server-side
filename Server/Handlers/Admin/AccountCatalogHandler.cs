using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;

namespace Server.Handlers.Admin;

public class AccountCatalogHandler : ISingletonScript
{
    private readonly AccountService _accountService;

    public AccountCatalogHandler(AccountService accountService)
    {
        _accountService = accountService;

        AltAsync.OnClient<ServerPlayer>("accountcatalog:open", OnOpenAccountCatalog);
    }

    private async void OnOpenAccountCatalog(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.AccountModel.Permission.HasFlag(Permission.STAFF))
        {
            return;
        }

        player.EmitGui("accountcatalog:open", await _accountService.GetAll());
    }
}