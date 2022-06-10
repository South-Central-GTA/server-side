using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.Bank;
using Server.Modules.Phone;

namespace Server.Handlers.Bank;

public class CreateBankAccountHandler : ISingletonScript
{
    private readonly BankAccountService _bankAccountService;

    private readonly BankModule _bankModule;
    private readonly PhoneModule _phoneModule;
    private readonly RegistrationOfficeService _registrationOfficeService;

    public CreateBankAccountHandler(BankAccountService bankAccountService,
        RegistrationOfficeService registrationOfficeService, BankModule bankModule, PhoneModule phoneModule)
    {
        _bankAccountService = bankAccountService;
        _registrationOfficeService = registrationOfficeService;

        _bankModule = bankModule;
        _phoneModule = phoneModule;

        AltAsync.OnClient<ServerPlayer, int, int>("phonebank:createaccount", OnCreateAccount);
    }

    private async void OnCreateAccount(ServerPlayer player, int phoneId, int bankAccountId)
    {
        if (!player.Exists)
        {
            return;
        }

        var isRegistered = await _registrationOfficeService.IsRegistered(player.CharacterModel.Id);
        if (!isRegistered)
        {
            await _phoneModule.SendNotification(phoneId, PhoneNotificationType.MAZE_BANK,
                "Leider können wir Sie unter Ihrem Namen nicht im Registration Office finden. Die Accounterstellung wurde abgebrochen.");
            return;
        }

        var bankAccounts = await _bankAccountService.GetByOwner(player.CharacterModel.Id);
        if (bankAccounts.Count >= 10)
        {
            await _phoneModule.SendNotification(phoneId, PhoneNotificationType.MAZE_BANK,
                "Leider können Sie keine weiteren Bankkonten bei uns eröffnen.");
            return;
        }

        if (bankAccounts.Count == 0)
        {
            await _bankModule.CreateBankAccount(player);
            await _bankModule.UpdateUi(player);
        }
        else
        {
            var bankAccount = await _bankAccountService.GetByKey(bankAccountId);
            if (bankAccount == null)
            {
                await _phoneModule.SendNotification(phoneId, PhoneNotificationType.MAZE_BANK,
                    "Wir konnten unter dieser Adresse leider kein Konto finden was auf Ihrem Namen registriert ist, Ihre Anfrage wurde vermerkt und abgelehnt.");
                return;
            }

            if (!await _bankModule.HasPermission(player, bankAccount, BankingPermission.TRANSFER))
            {
                await _phoneModule.SendNotification(phoneId, PhoneNotificationType.MAZE_BANK,
                    "Wir konnten leider nicht sicherstellen, das Sie auch Transfer Rechte auf dem angegebenen Bankkonto besitzen. Bitte prüfen Sie die Einstellungen ihres angegebenen Bankkontos.");
                return;
            }

            if (bankAccount.Amount < 500)
            {
                await _phoneModule.SendNotification(phoneId, PhoneNotificationType.MAZE_BANK,
                    $"Leider reicht Ihr aktuelles Guthaben auf dem Konto {bankAccount.BankDetails} nicht aus, Ihre Anfrage wurde vermerkt und abgelehnt.");
                return;
            }

            await _bankModule.Withdraw(bankAccount, 500, false, "Bankkonto Gründungskosten");
            await _bankModule.CreateBankAccount(player);
            await _bankModule.UpdateUi(player);
        }
    }
}