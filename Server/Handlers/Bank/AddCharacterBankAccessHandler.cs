using System.Linq;
using AltV.Net;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Banking;
using Server.Modules.Bank;
using Server.Modules.Phone;

namespace Server.Handlers.Bank;

public class AddCharacterBankAccessHandler : ISingletonScript
{
    private readonly PhoneModule _phoneModule;
    private readonly BankModule _bankModule;
    private readonly BankAccountCharacterAccessService _bankAccountCharacterAccessService;
    private readonly BankAccountService _bankAccountService;
    private readonly CharacterService _characterService;
    private readonly GroupService _groupService;

    public AddCharacterBankAccessHandler(
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
        
        AltAsync.OnClient<ServerPlayer, int, int, string>("bank:addcharacteraccess", OnAddCharacterAccess);
    }

    private async void OnAddCharacterAccess(ServerPlayer player, int phoneId, int bankAccountId, string characterName)
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
                                                "Sie können keine Management Aufträge für dieses Konto einreichen.");
            return;
        }

        var character = await _characterService.Find(c => c.FirstName + " " + c.LastName == characterName);
        if (character == null)
        {
            await _phoneModule.SendNotification(phoneId,
                                                PhoneNotificationType.MAZE_BANK,
                                                "Es konnte keine Person unter diesen Namen gefunden werden, wir konnten Niemanden zu Ihrem Bankkonto hinzufügen, wir entschuldigen die Unannehmlichkeiten.");
            return;
        }

        if (player.CharacterModel.Id == character.Id)
        {
            await _phoneModule.SendNotification(phoneId,
                                                PhoneNotificationType.MAZE_BANK,
                                                "Sie können sich nicht selbst auf ein Bankkonto hinzufügen, Sie haben schon Zugriffsrechte.");
            return;
        }

        foreach (var groupAccess in bankAccount.GroupRankAccess)
        {
            var group = await _groupService.GetByKey(groupAccess.GroupModelId);

            var member = group.Members.Find(m => m.CharacterModelId == character.Id);
            if (member != null)
            {
                if (member.Owner)
                {
                    await _phoneModule.SendNotification(phoneId,
                                                        PhoneNotificationType.MAZE_BANK,
                                                        "Sie können diese Person nicht zum Bankkonto hinzufügen, da sie schon Eigentümer einer Gruppe mit Zugriffsrechten ist.");
                    return;
                }

                await _phoneModule.SendNotification(phoneId,
                                                    PhoneNotificationType.MAZE_BANK,
                                                    "Sie können diese Person nicht zum Bankkonto hinzufügen, da sie schon Mitglied einer Gruppe mit Zugriffsrechten ist.");
                return;
            }
        }

        var characterAccess =
            bankAccount.CharacterAccesses.FirstOrDefault(ca => ca.CharacterModelId == character.Id);
        if (characterAccess != null)
        {
            if (characterAccess.Owner)
            {
                await _phoneModule.SendNotification(phoneId,
                                                    PhoneNotificationType.MAZE_BANK,
                                                    "Der angegebene Name ist schon als Eigentümer hinterlegt und kann daher keine Berechtigungen haben, da der Eigentümer den Vollzugriff hat.");
                return;
            }

            await _phoneModule.SendNotification(phoneId,
                                                PhoneNotificationType.MAZE_BANK,
                                                "Der angegebene Name hat schon Zugriff auf dieses Bankkonto, stellen Sie die Berechtigungen in der App ein.");
            return;
        }

        await _bankAccountCharacterAccessService.Add(new BankAccountCharacterAccessModel { CharacterModelId = character.Id, BankAccountModelId = bankAccount.Id });

        await _bankModule.UpdateUi(player);

        var targetPlayer = Alt.GetAllPlayers().FindPlayerByCharacterId(character.Id);
        if (targetPlayer != null)
        {
            await _bankModule.UpdateUi(targetPlayer);
        }

        await _phoneModule.SendNotification(phoneId,
                                            PhoneNotificationType.MAZE_BANK,
                                            $"Wir haben erfolgreich {character.Name} auf Ihr Bankkonto freigeschaltet, richten Sie nun bitte die Berechtigungen in der App ein.");
    }
}