using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;

namespace Server.Handlers.Admin;

public class MailsLogHandler : ISingletonScript
{
    private readonly MailService _mailService;

    public MailsLogHandler(MailService mailService)
    {
        _mailService = mailService;
        AltAsync.OnClient<ServerPlayer>("mailslog:open", OnOpenMailsLog);
    }

    private async void OnOpenMailsLog(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.AccountModel.Permission.HasFlag(Permission.STAFF))
        {
            return;
        }

        player.EmitGui("mailslog:setup", await _mailService.GetAll());
    }
}