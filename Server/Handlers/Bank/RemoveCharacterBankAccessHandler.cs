using System.Linq;
using AltV.Net;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.Bank;
using Server.Modules.Phone;

namespace Server.Handlers.Bank;

public class RemoveCharacterBankAccessHandler : ISingletonScript
{
    private readonly PhoneModule _phoneModule;
    private readonly BankModule _bankModule;
    private readonly BankAccountCharacterAccessService _bankAccountCharacterAccessService;
    private readonly BankAccountService _bankAccountService;
    private readonly CharacterService _characterService;
    private readonly GroupService _groupService;

    public RemoveCharacterBankAccessHandler(
        PhoneModule phoneModule,
        BankModule bankModule, 
        BankAccountCharacterAccessService bankAccountCharacterAccessService, 
        BankAccountService bankAccountService, 
        CharacterService characterService, 
        GroupService groupService)
    {
        _phoneModule = phoneModule;
        _bankModule = bankModule;
        _bankAccountCharacterAccessService = bankAccountCharacterAccessService;
        _bankAccountService = bankAccountService;
        _characterService = characterService;
        _groupService = groupService;
        
        AltAsync.OnClient<ServerPlayer, int, int, int>("bank:removeaccess", OnRemoveAccess);
    }

    private async void OnRemoveAccess(ServerPlayer player, int phoneId, int bankAccountId, int characterId)
    {
        if (!player.Exists)
        {
            return;
        }

        var bankAccount = await _bankAccountService.GetByKey(bankAccountId);
        if (bankAccount == null)
        {
            player.SendNotification("Das Bankkonto existiert nicht mehr.", NotificationType.ERROR);
            return;
        }

        if (!await _bankModule.HasPermission(player, bankAccount, BankingPermission.MANAGEMENT))
        {
            await _phoneModule.SendNotification(phoneId,
                                                PhoneNotificationType.MAZE_BANK,
                                                "Leider sind unter Ihren Namen nicht genügen Zugriffsrechte gesetzt, Sie können keine Management Aufträge für dieses Konto einreichen.");
            return;
        }

        foreach (var groupAccess in bankAccount.GroupRankAccess)
        {
            var group = await _groupService.GetByKey(groupAccess.GroupModelId);

            var member = group.Members.Find(m => m.CharacterModelId == characterId);
            if (member == null)
            {
                continue;
            }

            if (member.Owner)
            {
                await _phoneModule.SendNotification(phoneId,
                                                    PhoneNotificationType.MAZE_BANK,
                                                    "Sie können diese Person nicht von dem Bankkonto entfernen, da sie der Eigentümer einer Gruppe mit Zugriffsrechten ist.");
                return;
            }
        }

        var characterAccess =
            bankAccount.CharacterAccesses.FirstOrDefault(ca => ca.CharacterModelId == characterId);
        if (characterAccess is { Owner: true })
        {
            await _phoneModule.SendNotification(phoneId,
                                                PhoneNotificationType.MAZE_BANK,
                                                "Der angegebene Name ist als Eigentümer hinterlegt und kann daher nicht entfernt werden.");
            return;
        }

        var character = await _characterService.GetByKey(characterId);
        if (character == null)
        {
            return;
        }

        await _bankAccountCharacterAccessService.Remove(characterAccess);

        await _bankModule.UpdateUi(player);

        var targetPlayer = Alt.GetAllPlayers().FindPlayerByCharacterId(character.Id);
        if (targetPlayer != null)
        {
            await _bankModule.UpdateUi(targetPlayer);
        }

        await _phoneModule.SendNotification(phoneId,
                                            PhoneNotificationType.MAZE_BANK,
                                            $"Wir haben erfolgreich {character.Name} von Ihrem Bankkonto entfernt.");
    }
}