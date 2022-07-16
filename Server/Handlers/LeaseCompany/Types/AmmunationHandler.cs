using System.Linq;
using System.Threading.Tasks;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Housing;
using Server.Modules.Bank;
using Server.Modules.Inventory;
using Server.Modules.Money;
using Server.Modules.Narrator;
using Server.Modules.Weapon;

namespace Server.Handlers.LeaseCompany.Types;

public class AmmunationHandler : ISingletonScript
{
    private readonly BankAccountService _bankAccountService;
    private readonly BankModule _bankModule;
    private readonly HouseService _houseService;
    private readonly InventoryModule _inventoryModule;
    private readonly ItemCatalogService _itemCatalogService;
    private readonly ItemCreationModule _itemCreationModule;
    private readonly NarratorModule _narratorModule;

    private readonly MoneyModule _moneyModule;

    public AmmunationHandler(ItemCatalogService itemCatalogService, HouseService houseService,
        BankAccountService bankAccountService, MoneyModule moneyModule, BankModule bankModule,
        InventoryModule inventoryModule, ItemCreationModule itemCreationModule, NarratorModule narratorModule)
    {
        _itemCatalogService = itemCatalogService;
        _houseService = houseService;
        _bankAccountService = bankAccountService;

        _moneyModule = moneyModule;
        _bankModule = bankModule;
        _inventoryModule = inventoryModule;
        _itemCreationModule = itemCreationModule;
        _narratorModule = narratorModule;

        AltAsync.OnClient<ServerPlayer>("ammunation:requestopenmenu", OnRequestOpenMenu);
        AltAsync.OnClient<ServerPlayer, ItemCatalogIds, int>("ammunation:buyitem", OnBuyItem);
        AltAsync.OnClient<ServerPlayer, int, ItemCatalogIds, int, int>("ammunation:buywithcash", OnBuyWithCash);
        AltAsync.OnClient<ServerPlayer, int, ItemCatalogIds, int, int>("ammunation:buywithbank", OnBuyWithBank);
    }

    private async void OnRequestOpenMenu(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }
        
        if (!player.CharacterModel.Licenses.Exists(l => l.Type == PersonalLicensesType.WEAPON))
        {
            _narratorModule.SendMessage(player, "Der Verkäufer verlangt von deinem Charakter eine gültige Waffenlizenz. " +
                                              "Als dein Charakter keine vorlegen konnte verwies er deinen Charakter aus dem Laden.");
            return;
        }

        if (await _houseService.GetByDistance(player.Position, 20) is not LeaseCompanyHouseModel leaseCompanyHouse)
        {
            player.SendNotification("Hier kannst du nicht einkaufen.", NotificationType.ERROR);
            return;
        }

        if (!leaseCompanyHouse.HasOpen)
        {
            player.SendNotification("Dieser Laden hat geschlossen.", NotificationType.ERROR);
            return;
        }

        var buyableItems = await _itemCatalogService.Where(i => i.Buyable);
        buyableItems = buyableItems.Where(i =>
            WeaponModule.IsItemWeapon(i.Id) || AmmoModule.IsItemAmmo(i.Id) ||
            AttachmentModule.IsItemWeaponComponent(i.Id)).ToList();

        player.EmitLocked("ammunation:openmenu", buyableItems);
    }

    private async void OnBuyItem(ServerPlayer player, ItemCatalogIds catalogItemId, int amount)
    {
        var catalogItem = await _itemCatalogService.GetByKey(catalogItemId);

        if (!await _inventoryModule.CanCarry(player, catalogItem.Id, amount))
        {
            return;
        }

        var costs = catalogItem.Price * amount;

        var data = new object[3];
        data[0] = catalogItemId;
        data[1] = costs;
        data[2] = amount;

        player.CreateDialog(new DialogData
        {
            Type = DialogType.TWO_BUTTON_DIALOG,
            Title = "Waffenladen",
            Description =
                $"Du würdest für {amount}x {catalogItem.Name} <b>${costs}</b>, bezahlen.<br><p class='text-muted'>Du kannst mit dem Bargeld deines Charakters bezahlen oder per Banküberweisung.</p>",
            HasBankAccountSelection = true,
            FreezeGameControls = true,
            Data = data,
            PrimaryButton = "Bargeld nutzen",
            PrimaryButtonServerEvent = "ammunation:buywithcash",
            SecondaryButton = "Karte nutzen",
            SecondaryButtonServerEvent = "ammunation:buywithbank"
        });
    }

    private async void OnBuyWithCash(ServerPlayer player, int bankAccountId, ItemCatalogIds itemCatalogIds, int price,
        int amount)
    {
        var success = await _moneyModule.WithdrawAsync(player, price);
        if (success)
        {
            await TransactionSuccessfully(player, itemCatalogIds, amount);
        }
        else
        {
            player.SendNotification("Dein Charakter hat nicht genug Bargeld.", NotificationType.ERROR);
        }
    }

    private async void OnBuyWithBank(ServerPlayer player, int bankAccountId, ItemCatalogIds itemCatalogIds, int price,
        int amount)
    {
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

        var success = await _bankModule.Withdraw(bankAccount, price, false, "Ammu-Nation");
        if (success)
        {
            await TransactionSuccessfully(player, itemCatalogIds, amount);
        }
        else
        {
            player.SendNotification("Dein Charakter hat nicht genug Geld auf dem Bankkonto für diesen Einkauf.",
                NotificationType.ERROR);
        }
    }

    private async Task TransactionSuccessfully(ServerPlayer player, ItemCatalogIds catalogItemId, int amount)
    {
        var catalogItem = await _itemCatalogService.GetByKey(catalogItemId);

        if (!await _inventoryModule.CanCarry(player, catalogItem.Id, amount))
        {
            return;
        }

        if (!catalogItem.Stackable)
        {
            for (var i = 0; i < amount; i++)
            {
                await _itemCreationModule.AddItemAsync(player, catalogItemId, 1);
            }
        }
        else
        {
            var item = await _itemCreationModule.AddItemAsync(player, catalogItemId, amount);
            if (item == null)
            {
                return;
            }
        }

        await _inventoryModule.UpdateInventoryUiAsync(player);
        player.SendNotification("Erfolgreich bezahlt und Waren erhalten.", NotificationType.SUCCESS);
    }
}