using System.Linq;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Modules.Mail;

namespace Server.Handlers.Mail;

public class AddAccessMailAccountHandler : ISingletonScript
{
    private readonly CharacterService _characterService;
    private readonly GroupService _groupService;
    private readonly MailAccountService _mailAccountService;
    private readonly MailModule _mailModule;

    public AddAccessMailAccountHandler(
        MailAccountService mailAccountService,
        CharacterService characterService,
        GroupService groupService,
        MailModule mailModule)
    {
        _mailAccountService = mailAccountService;
        _characterService = characterService;
        _groupService = groupService;

        _mailModule = mailModule;

        AltAsync.OnClient<ServerPlayer, string, string>("mailing:addcharacteraccess", OnAddCharacterAccess);
    }

    private async void OnAddCharacterAccess(ServerPlayer player, string mailAddress, string characterName)
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

        var character = await _characterService.Find(c => c.FirstName + " " + c.LastName == characterName);
        if (character == null)
        {
            player.EmitGui("mail:senderror",
                           "Es konnte keine Person unter diesen Namen gefunden werden, wir konnten Niemanden zu Ihrem Mailkonto hinzufügen, wir entschuldigen die Unannehmlichkeiten.");

            return;
        }

        if (player.CharacterModel.Id == character.Id)
        {
            player.EmitGui("mail:senderror", "Sie können sich nicht selbst auf ein Mailkonto hinzufügen, Sie haben schon Zugriffsrechte.");
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
                               "Sie können diese Person nicht zum Mailkonto hinzufügen, da sie schon Eigentümer einer Gruppe mit Zugriffsrechten ist.");
                return;
            }

            player.EmitGui("mail:senderror",
                           "Sie können diese Person nicht zum Mailkonto hinzufügen, da sie schon Mitglied einer Gruppe mit Zugriffsrechten ist.");
            return;
        }

        var characterAccess =
            mailAccount.CharacterAccesses.FirstOrDefault(ca => ca.CharacterModelId == character.Id);
        if (characterAccess != null)
        {
            if (characterAccess.Owner)
            {
                player.EmitGui("mail:senderror",
                               "Der angegebene Name ist schon als Eigentümer hinterlegt und kann daher keine Berechtigungen haben, da der Eigentümer den Vollzugriff hat.");
                return;
            }

            player.EmitGui("mail:senderror",
                           "Der angegebene Name hat schon Zugriff auf dieses Mailkonto, stellen Sie die Berechtigungen in der App ein.");
            return;
        }

        await _mailModule.AddCharacterToAccount(player, character, mailAccount);
    }
}