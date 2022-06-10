using System.Linq;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Modules.Mail;

namespace Server.Handlers.Mail;

public class RemoveAccessMailAccountHandler : ISingletonScript
{
    private readonly CharacterService _characterService;
    private readonly GroupService _groupService;
    private readonly MailAccountService _mailAccountService;
    private readonly MailModule _mailModule;

    public RemoveAccessMailAccountHandler(MailAccountService mailAccountService, CharacterService characterService,
        GroupService groupService, MailModule mailModule)
    {
        _mailAccountService = mailAccountService;
        _characterService = characterService;
        _groupService = groupService;

        _mailModule = mailModule;

        AltAsync.OnClient<ServerPlayer, string, int>("mailing:removecharacteraccess", OnRemoveCharacterAccess);
    }

    private async void OnRemoveCharacterAccess(ServerPlayer player, string mailAddress, int characterId)
    {
        if (!player.Exists)
        {
            return;
        }

        var mailAccount = await _mailAccountService.GetByKey(mailAddress);
        if (mailAccount == null)
        {
            player.SendNotification("Das Mailkonto existiert nicht mehr.", NotificationType.ERROR);
            return;
        }

        if (!await _mailModule.IsOwner(player, mailAccount))
        {
            return;
        }

        var character = await _characterService.Find(c => c.Id == characterId);
        if (character == null)
        {
            player.EmitGui("mail:senderror",
                "Es konnte keine Person unter diesen Namen gefunden werden, wir konnten Niemanden zu Ihrem Mailkonto hinzufügen, wir entschuldigen die Unannehmlichkeiten.");
            return;
        }

        foreach (var groupAccess in mailAccount.GroupAccess)
        {
            var group = await _groupService.GetByKey(groupAccess.GroupModelId);

            var member = group.Members.Find(m => m.CharacterModelId == character.Id);
            if (member == null)
            {
                continue;
            }

            if (member.Owner)
            {
                player.EmitGui("mail:senderror",
                    "Sie können diese Person nicht von dem Mailkonto entfernen, da sie der Eigentümer einer Gruppe mit Zugriffsrechten ist.");
                return;
            }
        }

        var characterAccess = mailAccount.CharacterAccesses.FirstOrDefault(ca => ca.CharacterModelId == character.Id);
        if (characterAccess == null)
        {
            return;
        }

        if (characterAccess.Owner)
        {
            player.EmitGui("mail:senderror",
                "Der angegebene Name ist als Eigentümer hinterlegt und kann daher nicht entfernt werden.");
            return;
        }

        await _mailModule.RemoveCharacterFromAccount(player, character, characterAccess);
    }
}