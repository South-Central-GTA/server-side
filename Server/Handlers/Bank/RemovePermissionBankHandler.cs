using System;
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

public class RemovePermissionBankHandler : ISingletonScript
{
    private readonly BankModule _bankModule;
    private readonly PhoneModule _phoneModule;
    private readonly BankAccountService _bankAccountService;
    private readonly CharacterService _characterService;
    private readonly RegistrationOfficeService _registrationOfficeService;
    public RemovePermissionBankHandler(
        BankModule bankModule, 
        PhoneModule phoneModule, 
        BankAccountService bankAccountService, 
        CharacterService characterService, 
        RegistrationOfficeService registrationOfficeService)
    {
        _bankModule = bankModule;
        _phoneModule = phoneModule;
        _bankAccountService = bankAccountService;
        _characterService = characterService;
        _registrationOfficeService = registrationOfficeService;

        AltAsync.OnClient<ServerPlayer, int, int, int, string>("bank:removepermission", OnRemovePermission);
    }

    private async void OnRemovePermission(ServerPlayer player, int phoneId, int bankAccountId, int characterId, string expectedPermission)
    {
        if (!player.Exists)
        {
            return;
        }
        
        var isRegistered = await _registrationOfficeService.IsRegistered(player.CharacterModel.Id);
        if (!isRegistered)
        {
            player.SendNotification("Dein Charakter ist nicht im Registration Office gemeldet.", NotificationType.ERROR);
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

        var success = await _bankModule.RemovePermission(bankAccountId, characterId, permission);
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