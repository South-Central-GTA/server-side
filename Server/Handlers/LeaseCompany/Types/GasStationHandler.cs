using AltV.Net;
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
using Server.Database.Models.Housing;
using Server.Helper;
using Server.Modules.Bank;
using Server.Modules.Inventory;
using Server.Modules.Money;
using Server.Modules.Vehicles;
using Server.Modules.WorldMarket;

namespace Server.Handlers.LeaseCompany.Types;

public class GasStationHandler : ISingletonScript
{
    private readonly BankAccountService _bankAccountService;
    private readonly BankModule _bankModule;
    private readonly CompanyOptions _companyOptions;
    private readonly GroupService _groupService;
    private readonly HouseService _houseService;
    private readonly InventoryModule _invenoryModule;
    private readonly MoneyModule _moneyModule;
    private readonly Serializer _serializer;
    private readonly UserShopDataService _userShopDataService;
    private readonly VehicleCatalogService _vehicleCatalogService;
    private readonly VehicleModule _vehicleModule;

    private readonly WorldMarketModule _worldMarketModule;

    public GasStationHandler(IOptions<CompanyOptions> companyOptions, Serializer serializer, HouseService houseService,
        BankAccountService bankAccountService, VehicleCatalogService vehicleCatalogService, GroupService groupService,
        UserShopDataService userShopDataService, WorldMarketModule worldMarketModule, VehicleModule vehicleModule,
        MoneyModule moneyModule, BankModule bankModule)
    {
        _companyOptions = companyOptions.Value;
        _serializer = serializer;

        _houseService = houseService;
        _bankAccountService = bankAccountService;
        _vehicleCatalogService = vehicleCatalogService;
        _groupService = groupService;
        _userShopDataService = userShopDataService;

        _worldMarketModule = worldMarketModule;
        _vehicleModule = vehicleModule;
        _moneyModule = moneyModule;
        _bankModule = bankModule;

        AltAsync.OnClient<ServerPlayer>("gasstation:requestopenmenu", OnRequestOpenMenu);
        AltAsync.OnClient<ServerPlayer, float>("gasstation:requestrefuel", OnRequestRefuel);
        AltAsync.OnClient<ServerPlayer>("gasstation:playerleftarea", OnPlayerLeftArea);
        AltAsync.OnClient<ServerPlayer, int>("gasstation:buywithcash", OnBuyWithCash);
        AltAsync.OnClient<ServerPlayer, int>("gasstation:buywithbank", OnBuyWithBank);
    }

    private async void OnRequestOpenMenu(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        if (await _houseService.GetByDistance(player.Position, 20) is not LeaseCompanyHouseModel leaseCompanyHouse)
        {
            player.SendNotification("Hier kannst du nicht tanken.", NotificationType.ERROR);
            return;
        }

        var shopData = await _userShopDataService.GetByKey(player.CharacterModel.Id);
        if (shopData == null || shopData.BillToPay == 0)
        {
            player.SendNotification("Dein Charakter hat keine offene Rechnung.", NotificationType.ERROR);
            return;
        }

        player.CreateDialog(new DialogData
        {
            Type = DialogType.TWO_BUTTON_DIALOG,
            Title = "Zapfseule",
            Description =
                $"Der Treibstoff kostet dich <b>${shopData.BillToPay}</b>, willst du diesen Betrag bezahlen?<br>" +
                "<p class='text-muted'>Du kannst mit dem Bargeld deines Charakters bezahlen oder per Banküberweisung.</p>",
            HasBankAccountSelection = true,
            FreezeGameControls = true,
            PrimaryButton = "Bargeld nutzen",
            PrimaryButtonServerEvent = "gasstation:buywithcash",
            SecondaryButton = "Karte nutzen",
            SecondaryButtonServerEvent = "gasstation:buywithbank"
        });
    }

    private async void OnRequestRefuel(ServerPlayer player, float fuel)
    {
        if (!player.Exists)
        {
            return;
        }

        if (await _houseService.GetByDistance(player.Position, 10) is not LeaseCompanyHouseModel leaseCompanyHouse)
        {
            player.SendNotification("Hier kannst du nicht tanken.", NotificationType.ERROR);
            return;
        }

        if (!player.GetData("INTERACT_VEHICLE_REFILL", out int vehicleDbId))
        {
            return;
        }

        var vehicle = Alt.GetAllVehicles().FindByDbId(vehicleDbId);
        if (vehicle is not { Exists: true })
        {
            return;
        }

        if (vehicle.DbEntity == null)
        {
            return;
        }

        if (fuel <= 1)
        {
            player.SendNotification("Du kannst nicht so wenig tanken.", NotificationType.ERROR);
            return;
        }

        var catalogVehicle = await _vehicleCatalogService.GetByKey(vehicle.DbEntity.Model.ToLower());
        if (catalogVehicle == null)
        {
            return;
        }

        if (fuel > catalogVehicle.MaxTank)
        {
            player.SendNotification("Du kannst nicht so viel tanken.", NotificationType.ERROR);
            return;
        }

        if (fuel >= catalogVehicle.MaxTank)
        {
            fuel = catalogVehicle.MaxTank;
        }

        var price = (int)(fuel * _worldMarketModule.FuelPrice[catalogVehicle.FuelType]);
        await _vehicleModule.SetVehicleFuel(player, vehicle, (int)(vehicle.Fuel + fuel));
        player.EmitLocked("gasstation:startdistancecheck");

        var shopData = await _userShopDataService.GetByKey(player.CharacterModel.Id);
        if (shopData != null)
        {
            shopData.BillToPay += price;
            await _userShopDataService.Update(shopData);
        }
        else
        {
            await _userShopDataService.Add(new UserShopDataModel
            {
                CharacterModelId = player.CharacterModel.Id, GotWarned = false, BillToPay = price
            });
        }

        player.SendNotification("Du hast des Fahrzeug erfolgreich getankt.", NotificationType.SUCCESS);
        player.DeleteData("INTERACT_VEHICLE_REFILL");
    }

    // When the player leaves the area after refueling.
    private async void OnPlayerLeftArea(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        var shopData = await _userShopDataService.GetByKey(player.CharacterModel.Id);
        if (shopData == null)
        {
            return;
        }

        var bill = shopData.BillToPay;

        //TODO: Add actual call to police here over mdc.
        player.SendNotification($"Debug: Information ans PD, Tankdiebstahl in Wert von {bill}$.",
            NotificationType.WARNING);

        await _userShopDataService.Remove(shopData);
    }

    private async void OnBuyWithCash(ServerPlayer player, int bankAccountId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (await _houseService.GetByDistance(player.Position, 20) is not LeaseCompanyHouseModel leaseCompanyHouse)
        {
            player.SendNotification("Hier kannst du nicht bezahlen.", NotificationType.ERROR);
            return;
        }

        var shopData = await _userShopDataService.GetByKey(player.CharacterModel.Id);
        if (shopData?.BillToPay == 0)
        {
            player.SendNotification("Dein Charakter hat keine offene Rechnung.", NotificationType.ERROR);
            return;
        }

        var success = await _moneyModule.WithdrawAsync(player, shopData.BillToPay);
        if (success)
        {
            player.SendNotification("Dein Charakter hat den Treibstoff bezahlt.", NotificationType.SUCCESS);
            player.EmitLocked("gasstation:stopdistancecheck");
            await _userShopDataService.Remove(shopData);
        }
        else
        {
            player.SendNotification("Dein Charakter hat nicht genug Bargeld.", NotificationType.ERROR);
        }
    }

    private async void OnBuyWithBank(ServerPlayer player, int bankAccountId)
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

        if (!leaseCompanyHouse.HasOpen && !leaseCompanyHouse.PlayerDuty)
        {
            player.SendNotification("Dieser Laden hat geschlossen.", NotificationType.ERROR);
            return;
        }

        var shopData = await _userShopDataService.GetByKey(player.CharacterModel.Id);
        if (shopData?.BillToPay == 0)
        {
            player.SendNotification("Dein Charakter hat keine offene Rechnung.", NotificationType.ERROR);
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
            player.SendNotification($"Dein Charakter hat keine Transferrechte für das Konto {bankAccount.BankDetails}.",
                NotificationType.ERROR);
            return;
        }

        var success = await _bankModule.Withdraw(bankAccount, shopData.BillToPay, false,
            $"{leaseCompanyHouse.SubName} {_companyOptions.Types[leaseCompanyHouse.LeaseCompanyType].Name}");
        if (success)
        {
            player.SendNotification("Dein Charakter hat den Treibstoff bezahlt.", NotificationType.SUCCESS);
            player.EmitLocked("gasstation:stopdistancecheck");
            await _userShopDataService.Remove(shopData);
        }
        else
        {
            player.SendNotification("Dein Charakter hat nicht genug Geld auf dem Bankkonto für diesen Einkauf.",
                NotificationType.ERROR);
        }
    }
}