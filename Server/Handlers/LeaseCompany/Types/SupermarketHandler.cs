using System.Linq;
using AltV.Net.Async;
using Microsoft.Extensions.Options;
using Server.Core.Configuration;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Housing;
using Server.Handlers.LeaseCompany.Types.Base;
using Server.Modules.Bank;
using Server.Modules.Clothing;
using Server.Modules.Inventory;
using Server.Modules.Money;
using Server.Modules.Weapon;

namespace Server.Handlers.LeaseCompany.Types;

public class SupermarketHandler
    : BaseItemShopHandler
{
    private readonly HouseService _houseService;
    private readonly ItemCatalogService _itemCatalogService;

    public SupermarketHandler(
        IOptions<CompanyOptions> companyOptions,
        ItemCatalogService itemCatalogService,
        HouseService houseService,
        BankAccountService bankAccountService,
        ItemService itemService,
        GroupService groupService,
        UserShopDataService userShopDataService,
        InventoryService inventoryService,
        InventoryModule inventoryModule,
        ItemCreationModule itemCreationModule,
        MoneyModule moneyModule,
        BankModule bankModule) : base(companyOptions,
                                      itemCatalogService,
                                     houseService,
                                     bankAccountService,
                                     itemService,
                                     groupService,
                                     userShopDataService,
                                     inventoryService,
                                     inventoryModule,
                                     itemCreationModule,
                                     moneyModule,
                                     bankModule)
    {
        _itemCatalogService = itemCatalogService;
        _houseService = houseService;

        AltAsync.OnClient<ServerPlayer>("supermarket:requestopenmenu", OnRequestOpenMenu);
        AltAsync.OnClient<ServerPlayer, ItemCatalogIds, int>("supermarket:buyitem", OnBuyItem);
        AltAsync.OnClient<ServerPlayer>("supermarket:requestopencashiermenu", OnRequestOpenCashierMenu);
        AltAsync.OnClient<ServerPlayer, int>("supermarket:buywithcash", OnBuyWithCash);
        AltAsync.OnClient<ServerPlayer, int>("supermarket:buywithbank", OnBuyWithBank);
        AltAsync.OnClient<ServerPlayer>("supermarket:requestreturnitems", OnRequestReturnItems);
    }

    private async void OnRequestOpenMenu(ServerPlayer player)
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

        var buyableItems = await _itemCatalogService.Where(i => i.Buyable);
        buyableItems = buyableItems.Where(i => !ClothingModule.IsClothesOrProp(i.Id)
                                               && !WeaponModule.IsItemWeapon(i.Id)
                                               && !AmmoModule.IsItemAmmo(i.Id)
                                               && !AttachmentModule.IsItemWeaponComponent(i.Id)).ToList();

        player.EmitLocked("supermarket:openmenu", buyableItems);
    }

    private async void OnRequestOpenCashierMenu(ServerPlayer player)
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

        var costs = await GetBill(player);
        if (costs == 0)
        {
            player.SendNotification("Dein Charakter hat keine Waren von dem Supermarkt im Inventar.", NotificationType.ERROR);
            return;
        }

        player.CreateDialog(new DialogData
        {
            Type = DialogType.TWO_BUTTON_DIALOG,
            Title = "Supermarkt",
            Description = $"Deine ausgewählten Waren kosten <b>${costs}</b>, willst du diese bezahlen?<br><p class='text-muted'>Du kannst mit dem Bargeld deines Charakters bezahlen oder per Banküberweisung.</p>",
            HasBankAccountSelection = true,
            FreezeGameControls = true,
            PrimaryButton = "Bargeld nutzen",
            PrimaryButtonServerEvent = "supermarket:buywithcash",
            SecondaryButton = "Karte nutzen",
            SecondaryButtonServerEvent = "supermarket:buywithbank"
        });
    }
}