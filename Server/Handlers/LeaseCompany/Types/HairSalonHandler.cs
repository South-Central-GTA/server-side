using AltV.Net.Async;
using Microsoft.Extensions.Options;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Configuration;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Character;
using Server.Database.Models.Housing;
using Server.Helper;
using Server.Modules.Bank;
using Server.Modules.Money;

namespace Server.Handlers.LeaseCompany.Types;

public class HairSalonHandler : ISingletonScript
{
    private readonly CompanyOptions _companyOptions;
    private readonly BankAccountService _bankAccountService;
    private readonly BankModule _bankModule;
    private readonly CharacterService _characterService;
    private readonly GroupService _groupService;
    private readonly HouseService _houseService;

    private readonly MoneyModule _moneyModule;
    private readonly Serializer _serializer;

    public HairSalonHandler(
        IOptions<CompanyOptions> companyOptions,
        Serializer serializer,
        BankAccountService bankAccountService,
        GroupService groupService,
        HouseService houseService,
        CharacterService characterService,
        MoneyModule moneyModule,
        BankModule bankModule)
    {
        _companyOptions = companyOptions.Value;
        _serializer = serializer;
        _bankAccountService = bankAccountService;
        _groupService = groupService;
        _houseService = houseService;
        _characterService = characterService;

        _moneyModule = moneyModule;
        _bankModule = bankModule;

        AltAsync.OnClient<ServerPlayer>("hairsalon:requeststarthaircut", OnRequestStartHairCut);
        AltAsync.OnClient<ServerPlayer>("hairsalon:cancel", OnCancel);
        AltAsync.OnClient<ServerPlayer, string>("hairsalon:requestbuydialog", OnRequestBuyDialog);
        AltAsync.OnClient<ServerPlayer, int, string, int>("hairsalon:buywithcash", OnBuyWithCash);
        AltAsync.OnClient<ServerPlayer, int, string, int>("hairsalon:buywithbank", OnBuyWithBank);
    }

    private async void OnRequestStartHairCut(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        if (await _houseService.GetByDistance(player.Position, 20) is not LeaseCompanyHouseModel leaseCompanyHouse)
        {
            player.SendNotification("Hier kannst du dir nicht die Haare schneiden.", NotificationType.ERROR);
            return;
        }

        if (!leaseCompanyHouse.HasOpen && !leaseCompanyHouse.PlayerDuty)
        {
            player.SendNotification("Dieser Laden hat geschlossen.", NotificationType.ERROR);
            return;
        }

        await AltAsync.Do(() =>
        {
            player.EmitLocked("hairsalon:open", player.CharacterModel);
            player.SetUniqueDimension();
        });
    }

    private async void OnCancel(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        await player.SetDimensionAsync(0);
    }

    private async void OnRequestBuyDialog(ServerPlayer player, string newAppearancesJson)
    {
        if (!player.Exists)
        {
            return;
        }

        await player.SetDimensionAsync(0);

        var newAppearances = _serializer.Deserialize<AppearancesModel>(newAppearancesJson);
        var diff = player.CharacterModel.AppearancesModel.Diff(newAppearances);
        var price = diff * 100;

        var data = new object[2];
        data[0] = newAppearancesJson;
        data[1] = price;

        player.CreateDialog(new DialogData
        {
            Type = DialogType.TWO_BUTTON_DIALOG,
            Title = "Erscheinungsbild ändern",
            Description = $"Möchtest du dein neues Erscheinungsbild für <b>${price}</b> erwerben?<br><span class='text-muted'>Du kannst mit dem Bargeld deines Charakters bezahlen oder per Banküberweisung.</span>",
            HasBankAccountSelection = true,
            FreezeGameControls = true,
            Data = data,
            PrimaryButton = "Bargeld nutzen",
            PrimaryButtonServerEvent = "hairsalon:buywithcash",
            SecondaryButton = "Karte nutzen",
            SecondaryButtonServerEvent = "hairsalon:buywithbank"
        });
    }

    private async void OnBuyWithCash(ServerPlayer player, int bankAccountId, string newAppearancesJson, int price)
    {
        if (!player.Exists)
        {
            return;
        }

        if (await _houseService.GetByDistance(player.Position, 20) is not LeaseCompanyHouseModel leaseCompanyHouse)
        {
            player.SendNotification("Hier kannst du nicht einkaufen.", NotificationType.ERROR);
            return;
        }

        var success = await _moneyModule.WithdrawAsync(player, price);
        if (success)
        {
            player.CharacterModel.AppearancesModel.Update(_serializer.Deserialize<AppearancesModel>(newAppearancesJson));
            await _characterService.Update(player);

            player.EmitLocked("character:apply", player.CharacterModel);

            player.SendNotification("Dein Charakter konnte den Friseur Salon bezahlen.", NotificationType.SUCCESS);

            if (leaseCompanyHouse.GroupModelId.HasValue)
            {
                var owningGroup = await _groupService.GetByKey(leaseCompanyHouse.GroupModelId);
                if (owningGroup != null)
                {
                    var owningGroupBankAccount = await _bankAccountService.GetByOwningGroup(owningGroup.Id);
                    if (owningGroupBankAccount != null)
                    {
                        await _bankModule.Deposit(owningGroupBankAccount, price, "Dienstleistungen");
                    }
                }
            }
        }
        else
        {
            player.SendNotification("Dein Charakter hat nicht genug Bargeld.", NotificationType.ERROR);
        }
    }

    private async void OnBuyWithBank(ServerPlayer player, int bankAccountId, string newAppearancesJson, int price)
    {
        if (!player.Exists)
        {
            return;
        }

        if (string.IsNullOrEmpty(newAppearancesJson))
        {
            return;
        }

        if (await _houseService.GetByDistance(player.Position, 20) is not LeaseCompanyHouseModel leaseCompanyHouse)
        {
            player.SendNotification("Hier kannst du dein Erscheinungsbild nicht anpassen.", NotificationType.ERROR);
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
            player.SendNotification($"Dein Charakter hat keine Transferrechte für das Konto {bankAccount.BankDetails}.", NotificationType.ERROR);
            return;
        }

        var success = await _bankModule.Withdraw(bankAccount,
                                                 price,
                                                 false,
                                                 $"{leaseCompanyHouse.SubName} {_companyOptions.Types[leaseCompanyHouse.LeaseCompanyType].Name}");
        if (success)
        {
            player.CharacterModel.AppearancesModel.Update(_serializer.Deserialize<AppearancesModel>(newAppearancesJson));
            await _characterService.Update(player);

            player.SendNotification("Dein Charakter konnte den Friseur Salon bezahlen.", NotificationType.SUCCESS);
            player.EmitLocked("character:apply", player.CharacterModel);

            if (leaseCompanyHouse.GroupModelId.HasValue)
            {
                var owningGroup = await _groupService.GetByKey(leaseCompanyHouse.GroupModelId);
                if (owningGroup != null)
                {
                    var owningGroupBankAccount = await _bankAccountService.GetByOwningGroup(owningGroup.Id);
                    if (owningGroupBankAccount != null)
                    { 
                        await _bankModule.Deposit(owningGroupBankAccount, price, "Dienstleistungen");
                    }
                }
            }
        }
        else
        {
            player.SendNotification("Dein Charakter hat nicht genug Geld auf dem Bankkonto.", NotificationType.ERROR);
        }
    }
}