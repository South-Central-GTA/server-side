using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;

namespace Server.Handlers.Admin;

public class MailAccountsLog : ISingletonScript
{
    private readonly MailAccountService _mailAccountService;

    public MailAccountsLog(MailAccountService mailAccountService)
    {
        _mailAccountService = mailAccountService;

        AltAsync.OnClient<ServerPlayer>("mailaccountslog:open", OnOpenMailAccountsLog);
    }

    private async void OnOpenMailAccountsLog(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.AccountModel.Permission.HasFlag(Permission.STAFF))
        {
            return;
        }

        player.EmitGui("mailaccountslog:setup", await _mailAccountService.GetAll());
    }
}