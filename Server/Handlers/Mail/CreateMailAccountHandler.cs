using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Modules.Mail;

namespace Server.Handlers.Mail;

public class CreateMailAccountHandler : ISingletonScript
{
    private readonly MailModule _mailModule;

    public CreateMailAccountHandler(MailModule mailModule)
    {
        _mailModule = mailModule;

        AltAsync.OnClient<ServerPlayer, string>("mailing:createaccount", OnCreateAccount);
    }

    private async void OnCreateAccount(ServerPlayer player, string mailAddress)
    {
        await _mailModule.CreateMailAccount(player, mailAddress);
    }
}