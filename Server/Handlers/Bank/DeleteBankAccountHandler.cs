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
using Server.Modules.PublicGarage;

namespace Server.Handlers.Bank;

public class DeleteBankAccountHandler : ISingletonScript
{
    private readonly PhoneModule _phoneModule;
    private readonly BankModule _bankModule;
    private readonly PublicGarageModule _publicGarageModule;
    private readonly BankAccountService _bankAccountService;
    private readonly DefinedJobService _definedJobService;
    private readonly PublicGarageEntryService _publicGarageEntryService;
    private readonly RegistrationOfficeService _registrationOfficeService;

    public DeleteBankAccountHandler(
        PhoneModule phoneModule,
        BankModule bankModule,
        PublicGarageModule publicGarageModule,
        BankAccountService bankAccountService,
        DefinedJobService definedJobService,
        PublicGarageEntryService publicGarageEntryService,
        RegistrationOfficeService registrationOfficeService)
    {
        _phoneModule = phoneModule;
        _bankModule = bankModule;
        _publicGarageModule = publicGarageModule;
        _bankAccountService = bankAccountService;
        _definedJobService = definedJobService;
        _publicGarageEntryService = publicGarageEntryService;
        _registrationOfficeService = registrationOfficeService;

        AltAsync.OnClient<ServerPlayer, int, int>("bank:deletebankaccount", OnDeleteBankAccount);
    }

    private async void OnDeleteBankAccount(ServerPlayer player, int phoneId, int bankAccountId)
    {
        if (!player.Exists)
        {
            return;
        }

        var isRegistered = await _registrationOfficeService.IsRegistered(player.CharacterModel.Id);
        if (!isRegistered)
        {
            player.SendNotification("Dein Charakter ist nicht im Registration Office gemeldet.",
                                    NotificationType.ERROR);
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
            return;
        }

        if (bankAccount.GroupRankAccess.Count != 0)
        {
            if (bankAccount.GroupRankAccess.Count == 1)
            {
                await _phoneModule.SendNotification(phoneId,
                                                    PhoneNotificationType.MAZE_BANK,
                                                    $"Das Konto {bankAccount.BankDetails} wird als ein Gruppenkonto geführt und kann daher nicht gelöscht werden.");
            }
            else
            {
                await _phoneModule.SendNotification(phoneId,
                                                    PhoneNotificationType.MAZE_BANK,
                                                    $"Auf das Konto {bankAccount.BankDetails} haben noch Gruppen Zugriffsrechte, es kann nicht gelöscht werden.");
            }

            return;
        }

        var publicGarageEntry = await _publicGarageEntryService.Find(pge => pge.BankAccountId == bankAccount.Id);
        if (publicGarageEntry != null)
        {
            var publicGarageData = _publicGarageModule.FindGarage(publicGarageEntry.GarageId);
            await _phoneModule.SendNotification(phoneId,
                                                PhoneNotificationType.MAZE_BANK,
                                                $"Das Konto {bankAccount.BankDetails} wird noch bei {publicGarageData?.Name} verwendet, bitte kündigen Sie erst ihre Transaktionen dort.");
            return;
        }

        if (bankAccount.Amount != 0)
        {
            await _phoneModule.SendNotification(phoneId,
                                                PhoneNotificationType.MAZE_BANK,
                                                $"Das Konto {bankAccount.BankDetails} ist nicht genullt, es kann nicht geschlossen werden.");
            return;
        }

        var definedJob = await _definedJobService.Find(j => j.BankAccountId == bankAccount.Id);
        if (definedJob != null)
        {
            await _definedJobService.Remove(definedJob);
        }

        await _bankAccountService.Remove(bankAccount);

        await _phoneModule.SendNotification(phoneId,
                                            PhoneNotificationType.MAZE_BANK,
                                            $"Das Konto {bankAccount.BankDetails} wurde erfolgreich geschlossen.");

        // Update all player bank ui when there had access for this deleted bank account.
        foreach (var playerWithAccessRights
                 in bankAccount.CharacterAccesses.Select(bankAccountCharacterAccess
                                                             => Alt.GetAllPlayers()
                                                                   .FindPlayerByCharacterId(
                                                                       bankAccountCharacterAccess.CharacterModelId))
                               .Where(serverPlayer => serverPlayer != null))
        {
            await _bankModule.UpdateUi(playerWithAccessRights);
        }
    }
}