using System.Linq;
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
using Server.Modules.Mail;
using Server.Modules.Phone;

namespace Server.Handlers.Company;

public class CreateCompanyHandler : ISingletonScript
{
    private readonly BankAccountService _bankAccountService;
    private readonly BankModule _bankModule;
    private readonly CompanyOptions _companyOptions;
    private readonly GroupMemberService _groupMemberService;
    private readonly GroupModule _groupModule;
    private readonly GroupService _groupService;
    private readonly HouseService _houseService;
    private readonly MailModule _mailModule;
    private readonly PhoneModule _phoneModule;
    private readonly RegistrationOfficeService _registrationOfficeService;

    public CreateCompanyHandler(IOptions<CompanyOptions> companyOptions, GroupService groupService,
        HouseService houseService, BankAccountService bankAccountService, GroupMemberService groupMemberService,
        RegistrationOfficeService registrationOfficeService, GroupModule groupModule, PhoneModule phoneModule,
        BankModule bankModule, MailModule mailModule)
    {
        _companyOptions = companyOptions.Value;

        _groupService = groupService;
        _houseService = houseService;
        _bankAccountService = bankAccountService;
        _groupMemberService = groupMemberService;
        _registrationOfficeService = registrationOfficeService;

        _groupModule = groupModule;
        _phoneModule = phoneModule;
        _bankModule = bankModule;
        _mailModule = mailModule;

        AltAsync.OnClient<ServerPlayer, int, string, int, int>("company:create", OnCreateCompany);
    }

    private async void OnCreateCompany(ServerPlayer player, int phoneId, string name, int bankAccountId, int houseId)
    {
        var isRegistered = await _registrationOfficeService.IsRegistered(player.CharacterModel.Id);
        if (!isRegistered)
        {
            player.SendNotification("Dein Charakter ist nicht im Registration Office gemeldet.",
                NotificationType.ERROR);
            return;
        }

        if (await _groupModule.IsPlayerInGroupType(player, GroupType.COMPANY))
        {
            await _phoneModule.SendNotification(phoneId, PhoneNotificationType.GOV,
                "Leider konnten wir Ihre Unternehmens- und somit Existenzgründung nicht bearbeiten, da Sie sich schon laut Register in einem Unternehmen tätig sind.");
            return;
        }

        if (await _groupModule.IsPlayerInGroupType(player, GroupType.FACTION))
        {
            await _phoneModule.SendNotification(phoneId, PhoneNotificationType.GOV,
                "Leider konnten wir Ihre Unternehmens- und somit Existenzgründung nicht bearbeiten, da Sie sich schon laut Register in einer Fraktion tätig sind.");
            return;
        }

        var groups = await _groupService.GetByOwner(player.CharacterModel.Id);
        if (groups.Count != 0)
        {
            if (groups.Any(g => g.GroupType == GroupType.COMPANY))
            {
                await _phoneModule.SendNotification(phoneId, PhoneNotificationType.GOV,
                    "Die Anmeldung Ihres Unternehmens befindet sich aktuell noch in Bearbeitung, bitte haben Sie Verständnis das wir weitere Anfragen automatisch ablehnen müssen!");
                return;
            }
        }

        var house = await _houseService.GetByKey(houseId);
        if (house == null)
        {
            await _phoneModule.SendNotification(phoneId, PhoneNotificationType.GOV,
                "Die Anmeldung Ihres Unternehmens konnte leider nicht bearbeitet werden, da Ihre angegebene Adresse nicht gefunden wurde!");
            return;
        }

        if (house.GroupModelId.HasValue)
        {
            var houseGroup = await _groupService.GetByKey(house.GroupModelId.Value);
            if (houseGroup.GroupType == GroupType.COMPANY)
            {
                await _phoneModule.SendNotification(phoneId, PhoneNotificationType.GOV,
                    "Die Anmeldung Ihres Unternehmens konnte leider nicht bearbeitet werden, da Ihre angegebene Adresse schon für ein Unternehmen genutzt wird!");
            }
            else
            {
                await _phoneModule.SendNotification(phoneId, PhoneNotificationType.GOV,
                    "Die Anmeldung Ihres Unternehmens konnte leider nicht bearbeitet werden, da Ihre angegebene Adresse als nicht gültiger Bürositz klassifiziert wurde.");
            }

            return;
        }

        var bankAccount = await _bankAccountService.GetByKey(bankAccountId);
        if (bankAccount == null)
        {
            player.SendNotification("Das Bankkonto existiert nicht mehr.", NotificationType.ERROR);
            return;
        }

        if (!await _bankModule.HasPermission(player, bankAccount, BankingPermission.TRANSFER))
        {
            await _phoneModule.SendNotification(phoneId, PhoneNotificationType.GOV,
                "Die Anmeldung Ihres Unternehmens konnte leider nicht bearbeitet werden, da die Maze Bank keine Transaktionsberechtigung für das angegebene Bankkonto finden konnte.");
            return;
        }

        if (string.IsNullOrEmpty(name))
        {
            await _phoneModule.SendNotification(phoneId, PhoneNotificationType.GOV,
                "Die Anmeldung Ihres Unternehmens konnte leider nicht bearbeitet werden, da Sie keinen validen Namen als Unternehmen angegeben haben.");
            return;
        }

        if (player.CharacterModel.JobModel != null)
        {
            await _phoneModule.SendNotification(phoneId, PhoneNotificationType.GOV,
                "Die Anmeldung Ihres Unternehmens konnte leider nicht bearbeitet werden, da Sie beim Arbeitsamt noch als Arbeitsnehmer registriert sind.");
            return;
        }

        var success = await _bankModule.Withdraw(bankAccount, _companyOptions.CreateCosts, false,
            $"Unternehmensgründung '{name}'");
        if (success)
        {
            var createdGroup = await _groupModule.CreateGroup(GroupType.COMPANY, name);
            if (createdGroup != null)
            {
                await _groupMemberService.Add(new GroupMemberModel
                {
                    GroupModelId = createdGroup.Id,
                    CharacterModelId = player.CharacterModel.Id,
                    Owner = true,
                    BankAccountId = bankAccountId
                });

                house.CharacterModelId = null;
                house.GroupModelId = createdGroup.Id;
                await _houseService.Update(house);

                await _bankModule.CreateBankAccount(createdGroup);
                await _mailModule.CreateMailAccount(createdGroup);
                await _groupModule.CreateMemberInventory(player, createdGroup);

                await _groupService.Update(createdGroup);

                await _phoneModule.SendNotification(phoneId, PhoneNotificationType.GOV,
                    "Ihre Anfrage zur Unternehmens- und somit Existenzgründung, ging bei uns ein. Wir werden sie in kürze bearbeiten. Bitte haben Sie noch etwas Geduld.");
            }
            else
            {
                await _phoneModule.SendNotification(phoneId, PhoneNotificationType.GOV,
                    "Die Anmeldung Ihres Unternehmens konnte leider nicht bearbeitet werden, da der Name leider im Register schon vergeben ist.");
            }
        }
        else
        {
            await _phoneModule.SendNotification(phoneId, PhoneNotificationType.GOV,
                "Die Anmeldung Ihres Unternehmens konnte leider nicht bearbeitet werden, da die Maze Bank nicht genügend Geld auf dem angegebenen Bankkonto finden konnte.");
        }
    }
}