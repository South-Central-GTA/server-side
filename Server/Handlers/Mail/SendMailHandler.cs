using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Modules.Mail;

namespace Server.Handlers.Mail;

public class SendMailHandler : ISingletonScript
{
    private readonly MailModule _mailModule;

    public SendMailHandler(MailModule mailModule)
    {
        _mailModule = mailModule;

        AltAsync.OnClient<ServerPlayer, string, string, string, string>("mailing:sendmail", OnSendMail);
    }

    private async void OnSendMail(ServerPlayer player, string senderMailAddress, string targetMailAddress, string title, string content)
    {
        await _mailModule.SendMail(player, senderMailAddress, targetMailAddress, title, content);
    }
}