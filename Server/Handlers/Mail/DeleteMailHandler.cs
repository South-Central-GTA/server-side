using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Modules.Mail;

namespace Server.Handlers.Mail;

public class DeleteMailHandler : ISingletonScript
{
    private readonly MailModule _mailModule;

    public DeleteMailHandler(MailModule mailModule)
    {
        _mailModule = mailModule;

        AltAsync.OnClient<ServerPlayer, string, int>("mailing:deletemail", OnDeleteMail);
    }

    private async void OnDeleteMail(ServerPlayer player, string mailAddress, int mailId)
    {
        await _mailModule.DeleteMail(mailAddress, mailId);
    }
}