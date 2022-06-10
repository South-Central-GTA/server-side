using System.Linq;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;

namespace Server.Handlers.Admin;

public class PhoneMessageLogHandler : ISingletonScript
{
    private readonly PhoneMessageService _phoneMessagesService;

    public PhoneMessageLogHandler(PhoneMessageService phoneMessagesService)
    {
        _phoneMessagesService = phoneMessagesService;

        AltAsync.OnClient<ServerPlayer>("phonemessageslog:open", OnOpenPhoneMessagesLog);
    }

    private async void OnOpenPhoneMessagesLog(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.AccountModel.Permission.HasFlag(Permission.STAFF))
        {
            return;
        }

        var allMessages = await _phoneMessagesService.GetAll();
        var allUnique = allMessages.GroupBy(p => p.Context).Select(g => g.First()).ToList();

        player.EmitGui("phonemessageslog:open", allUnique);
    }
}