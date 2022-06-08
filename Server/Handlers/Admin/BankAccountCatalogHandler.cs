using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;

namespace Server.Handlers.Admin;

public class BankAccountCatalogHandler : ISingletonScript
{
    private readonly BankAccountService _bankAccountService;

    public BankAccountCatalogHandler(BankAccountService bankAccountService)
    {
        _bankAccountService = bankAccountService;

        AltAsync.OnClient<ServerPlayer>("bankaccountcatalog:open", OnOpen);
    }

    private async void OnOpen(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.AccountModel.Permission.HasFlag(Permission.STAFF))
        {
            return;
        }

        player.EmitGui("bankaccountcatalog:setup", await _bankAccountService.GetAll());
    }
}