using System;
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

public class AddPermissionBankHandler : ISingletonScript
{
    private readonly PhoneModule _phoneModule;
    private readonly BankModule _bankModule;
    private readonly BankAccountService _bankAccountService;
    private readonly CharacterService _characterService;

    public AddPermissionBankHandler(
        PhoneModule phoneModule, 
        BankModule bankModule, 
        BankAccountService bankAccountService, 
        CharacterService characterService)
    {
        _phoneModule = phoneModule;
        _bankModule = bankModule;
        _bankAccountService = bankAccountService;
        _characterService = characterService;
        
        AltAsync.OnClient<ServerPlayer, int, int, int, string>("bank:addpermission", OnAddPermission);
    }

    private async void OnAddPermission(ServerPlayer player, int phoneId, int bankAccountId, int characterId, string expectedPermission)
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

        var character = await _characterService.GetByKey(characterId);
        if (character == null)
        {
            return;
        }

        if (!Enum.TryParse(expectedPermission, true, out BankingPermission permission))
        {
            return;
        }

        var characterAccess =
            bankAccount.CharacterAccesses.FirstOrDefault(ca => ca.CharacterModelId == player.CharacterModel.Id);
        if (characterAccess != null)
        {
            if (permission == BankingPermission.MANAGEMENT && !characterAccess.Owner)
            {
                await _phoneModule.SendNotification(phoneId,
                                                    PhoneNotificationType.MAZE_BANK,
                                                    "Leider können Sie mit Ihrem Account keine anderen Accounts Management Berechtigungen geben.");
                return;
            }
        }

        var success = await _bankModule.AddPermission(bankAccountId, characterId, permission);
        if (!success)
        {
            return;
        }

        await _bankModule.UpdateUi(player);

        var targetPlayer = Alt.GetAllPlayers().FindPlayerByCharacterId(character.Id);
        if (targetPlayer != null)
        {
            await _bankModule.UpdateUi(targetPlayer);
        }
    }
}