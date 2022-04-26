using System.Linq;
using AltV.Net;
using AltV.Net.Async;
using Microsoft.Extensions.Options;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Configuration;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Group;
using Server.Modules.Bank;
using Server.Modules.Group;
using Server.Modules.Phone;

namespace Server.Handlers.Company;

public class BuyLicenseCompanyHandler : ISingletonScript
{
    private readonly CompanyOptions _companyOptions;
    private readonly BankAccountService _bankAccountService;
    private readonly GroupService _groupService;
    private readonly RegistrationOfficeService _registrationOfficeService;
    
    private readonly BankModule _bankModule;
    private readonly GroupModule _groupModule;
    private readonly PhoneModule _phoneModule;  

    public BuyLicenseCompanyHandler(
        IOptions<CompanyOptions> companyOptions,
        GroupService groupService,
        BankAccountService bankAccountService,
        RegistrationOfficeService registrationOfficeService,
        
        GroupModule groupModule,
        PhoneModule phoneModule,
        BankModule bankModule)
    {
        _companyOptions = companyOptions.Value;

        _groupService = groupService;
        _bankAccountService = bankAccountService;
        _registrationOfficeService = registrationOfficeService;

        _groupModule = groupModule;
        _phoneModule = phoneModule;
        _bankModule = bankModule;

        AltAsync.OnClient<ServerPlayer, int, int, LicensesFlags>("company:buylicenses", OnBuyLicenses);
    }

    private async void OnBuyLicenses(ServerPlayer player, int phoneId, int companyId, LicensesFlags licensesFlags)
    {
        var groups = await _groupService.GetGroupsByCharacter(player.CharacterModel.Id);
        var group = groups?.FirstOrDefault(g => g.GroupType == GroupType.COMPANY && g.Id == companyId);
        if (group == null)
        {
            return;
        }
        
        var companyGroup = (CompanyGroupModel)group;

        if (!await _groupModule.HasPermission(player.CharacterModel.Id, group.Id, GroupPermission.BUY_LICENSES))
        {
            return;
        }
        
        var isRegistered = await _registrationOfficeService.IsRegistered(player.CharacterModel.Id);
        if (!isRegistered)
        {
            player.SendNotification("Dein Charakter ist nicht im Registration Office gemeldet.", NotificationType.ERROR);
            return;       
        }

        if (companyGroup.PurchasedLicenses + 1 > _companyOptions.MaxLicenses)
        {
            await _phoneModule.SendNotification(phoneId,
                                                PhoneNotificationType.GOV,
                                                "Leider konnten wir keine weiteren Lizenzen zu Ihrem Unternehmen hinzufügen. " +
                                                $"Das aktuell maximale Limit beträgt {_companyOptions.MaxLicenses} Lizenzen.");
            return;
        }

        var license = _companyOptions.Licenses.Find(l => l.License == licensesFlags);
        if (license == null)
        {
            return;
        }

        var bankAccount = await _bankAccountService.Find(ba =>
                                                             ba.Type == OwnableAccountType.GROUP
                                                             && ba.GroupRankAccess.Any(gra => gra.GroupModelId == companyGroup.Id));

        if (!await _bankModule.HasPermission(player, bankAccount, BankingPermission.TRANSFER))
        {
            await _phoneModule.SendNotification(phoneId,
                                                PhoneNotificationType.GOV,
                                                "Leider konnten wir Ihre Buchung für die Lizenzen nicht abschließen. " +
                                                $"Da Sie keine Transferrechte für das Konto {bankAccount.BankDetails} haben.");
            return;
        }

        var success = await _bankModule.Withdraw(bankAccount, license.Price, false, $"Lizenz '{license.Name}' gekauft");
        if (success)
        {
            companyGroup.LicensesFlags |= license.License;
            companyGroup.PurchasedLicenses++;

            foreach (var target in
                     group.Members.Select(m => Alt.GetAllPlayers().FindPlayerByCharacterId(m.CharacterModelId)))
            {
                await _bankModule.UpdateUi(target);
            }

            await _bankModule.UpdateUi(player);

            await _groupService.Update(companyGroup);
            await _groupModule.UpdateGroupUi(companyGroup);
        }
        else
        {
            await _phoneModule.SendNotification(phoneId,
                                                PhoneNotificationType.GOV,
                                                "Leider konnten wir Ihre Buchung für die Lizenzen nicht abschließen. " +
                                                "Da ihr Unternehmenskonto nicht genügend Guthaben aufweist.");
        }
    }
}