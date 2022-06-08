using AltV.Net;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Banking;
using Server.Modules.Bank;
using Server.Modules.Group;
using Server.Modules.Inventory;
using Server.Modules.Money;
using Server.Modules.Vehicles;

namespace Server.Handlers.Vehicle;

public class VehicleSellHandler : ISingletonScript
{
    private readonly BankAccountService _bankAccountService;
    private readonly BankModule _bankModule;
    private readonly GroupModule _groupModule;
    private readonly InventoryModule _inventoryModule;
    private readonly MoneyModule _moneyModule;

    private readonly VehicleModule _vehicleModule;

    public VehicleSellHandler(
        VehicleModule vehicleModule,
        MoneyModule moneyModule,
        BankModule bankModule,
        InventoryModule inventoryModule,
        GroupModule groupModule,
        BankAccountService bankAccountService)
    {
        _vehicleModule = vehicleModule;
        _moneyModule = moneyModule;
        _bankModule = bankModule;
        _inventoryModule = inventoryModule;
        _groupModule = groupModule;

        _bankAccountService = bankAccountService;

        AltAsync.OnClient<ServerPlayer, int>("vehiclemenu:sell", OnSellVehicle);

        AltAsync.OnClient<ServerPlayer, int, int>("vehiclesellmenu:requestcash", OnRequestCashSell);
        AltAsync.OnClient<ServerPlayer, int, int, int>("vehiclesellmenu:requestbank", OnRequestBankSell);

        AltAsync.OnClient<ServerPlayer, int, int>("vehicle:buyovervsellwithcash", OnBuyOverVSellWithCash);
        AltAsync.OnClient<ServerPlayer, int, int, int, int>("vehicle:buyovervsellwithbank", OnBuyOverVSellWithBank);
    }

    private async void OnSellVehicle(ServerPlayer player, int vehicleDbId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (player.IsInVehicle)
        {
            player.SendNotification("Dein Charakter darf in keinem Fahrzeug sitzen.", NotificationType.ERROR);
            return;
        }

        var vehicle = Alt.GetAllVehicles().FindByDbId(vehicleDbId);
        if (vehicle is not { Exists: true })
        {
            player.SendNotification("Dieses Fahrzeug kannst du nicht verkaufen.", NotificationType.ERROR);
            return;
        }

        var hasBankAccount = await _bankModule.HasBankAccount(player);
        var isGroupVehicle = vehicle.DbEntity.GroupModelOwnerId.HasValue;

        if (isGroupVehicle)
        {
            if (!await _groupModule.HasPermission(player.CharacterModel.Id,
                                                  vehicle.DbEntity.GroupModelOwnerId.Value,
                                                  GroupPermission.SELL_VEHICLES))
            {
                player.SendNotification("Dein Charakter hat dafür nicht genügend Berechtigungen in der Gruppe.",
                                        NotificationType.ERROR);
                return;
            }

            player.SetData("VEHICLE_SELL_GROUP_ID", vehicle.DbEntity.GroupModelOwnerId);
        }
        else
        {
            if (!vehicle.DbEntity.CharacterModelId.HasValue ||
                vehicle.DbEntity.CharacterModelId.Value != player.CharacterModel.Id)
            {
                player.SendNotification("Du bist nicht der Eigentümer des Fahrzeuges.", NotificationType.ERROR);
                return;
            }

            player.SetData("VEHICLE_SELL_OWNER_ID", vehicle.DbEntity.CharacterModelId);
        }

        player.SetData("VEHICLE_SELL_VEHICLE_ID", vehicleDbId);

        player.EmitLocked("vehiclesellmenu:show", hasBankAccount, isGroupVehicle);
    }

    private void OnRequestCashSell(ServerPlayer player, int targetPlayerId, int price)
    {
        if (!player.Exists)
        {
            return;
        }

        player.EmitLocked("vehiclesellmenu:close");

        var target = Alt.GetAllPlayers().GetPlayerById(player, targetPlayerId);
        if (target == null)
        {
            return;
        }

        if (player == target)
        {
            player.SendNotification("Du kannst ein Fahrzeug nicht an dir selbst verkaufen.", NotificationType.ERROR);
            return;
        }

        if (price < 0)
        {
            player.SendNotification("Der Preis darf nicht Negativ sein.", NotificationType.ERROR);
            return;
        }

        var data = new object[2];
        data[0] = player.Id;
        data[1] = price;

        target.CreateDialog(new DialogData
        {
            Type = DialogType.ONE_BUTTON_DIALOG,
            Title = "Fahrzeug Angebot",
            Description = $"Möchtest du dieses Fahrzeug für <b>${price}</b> in Bar erwerben?",
            FreezeGameControls = true,
            Data = data,
            PrimaryButton = "Fahrzeug kaufen",
            PrimaryButtonServerEvent = "vehicle:buyovervsellwithcash"
        });
    }

    private void OnRequestBankSell(ServerPlayer player, int targetPlayerId, int price, int bankAccountId)
    {
        if (!player.Exists)
        {
            return;
        }

        player.EmitLocked("vehiclesellmenu:close");

        var target = Alt.GetAllPlayers().GetPlayerById(player, targetPlayerId);
        if (target == null)
        {
            return;
        }

        if (player == target)
        {
            player.SendNotification("Du kannst ein Fahrzeug nicht an dir selbst verkaufen.", NotificationType.ERROR);
            return;
        }

        if (price < 0)
        {
            player.SendNotification("Der Preis darf nicht Negativ sein.", NotificationType.ERROR);
            return;
        }

        var data = new object[3];
        data[0] = price;
        data[1] = bankAccountId;
        data[2] = player.Id;

        target.CreateDialog(new DialogData
        {
            Type = DialogType.ONE_BUTTON_DIALOG,
            Title = "Fahrzeug Angebot",
            Description =
                $"Möchtest du dieses Fahrzeug für <b>${price}</b> per Banküberweisung erwerben?",
            HasBankAccountSelection = true,
            FreezeGameControls = true,
            Data = data,
            PrimaryButton = "Fahrzeug kaufen",
            PrimaryButtonServerEvent = "vehicle:buyovervsellwithbank"
        });
    }

    private async void OnBuyOverVSellWithCash(ServerPlayer player, int targetPlayerId, int price)
    {
        if (!player.Exists)
        {
            return;
        }

        var seller = Alt.GetAllPlayers().GetPlayerById(player, targetPlayerId);
        var vehicle = GetVehicleCheckIfSellerIsOwner(player, seller);
        if (vehicle == null)
        {
            return;
        }

        if (price < 0)
        {
            player.SendNotification("Beim Preis ist ein Fehler aufgetreten", NotificationType.ERROR);
            seller.SendNotification("Der Preis darf nicht negativ sein", NotificationType.ERROR);
            return;
        }

        if (player.Position.Distance(seller.Position) > 3)
        {
            player.SendNotification("Dein Charakter ist zu weit weg von dem anderen Charakter.",
                                    NotificationType.ERROR);
            seller.SendNotification("Dein Charakter ist zu weit weg von dem anderen Charakter.",
                                    NotificationType.ERROR);
            return;
        }

        if (!await _inventoryModule.CanCarry(player, ItemCatalogIds.KEY))
        {
            seller.SendNotification("Kauf konnte nicht abgeschlossen werden.", NotificationType.ERROR);
            return;
        }

        var success = await _moneyModule.WithdrawAsync(player, price);
        if (success)
        {
            await _moneyModule.GiveMoney(seller, price);

            await _vehicleModule.SetCharacterOwner(vehicle, player);

            player.SendNotification("Kauf erfolgreich abgeschlossen.", NotificationType.SUCCESS);
            seller.SendNotification("Kauf erfolgreich abgeschlossen.", NotificationType.SUCCESS);
        }
        else
        {
            player.SendNotification("Dein Charakter hat nicht genug Bargeld.", NotificationType.ERROR);
            seller.SendNotification("Kauf konnte nicht abgeschlossen werden.", NotificationType.ERROR);
        }
    }

    private async void OnBuyOverVSellWithBank(ServerPlayer player, int bankAccountId, int price,
                                              int targetBankAccountId, int targetPlayerId)
    {
        if (!player.Exists)
        {
            return;
        }

        var seller = Alt.GetAllPlayers().GetPlayerById(player, targetPlayerId);
        if (seller == null)
        {
            return;
        }

        var vehicle = GetVehicleCheckIfSellerIsOwner(player, seller);
        if (vehicle == null)
        {
            return;
        }

        if (price < 0)
        {
            player.SendNotification("Beim Preis ist ein Fehler aufgetreten", NotificationType.ERROR);
            seller.SendNotification("Der Preis darf nicht negativ sein", NotificationType.ERROR);
            return;
        }

        var bankAccount = await _bankAccountService.GetByKey(bankAccountId);
        if (bankAccount == null)
        {
            player.SendNotification("Dein Charakter hat kein Bankkonto, der Kauf wurde abgebrochen.",
                                    NotificationType.ERROR);
            seller.SendNotification("Kauf konnte nicht abgeschlossen werden.", NotificationType.ERROR);
            return;
        }

        if (!await _bankModule.HasPermission(player, bankAccount, BankingPermission.TRANSFER))
        {
            player.SendNotification("Dein Charakter hat keine Überweisungsrechte auf dem Bankkonto.",
                                    NotificationType.ERROR);
            seller.SendNotification("Kauf konnte nicht abgeschlossen werden.", NotificationType.ERROR);
            return;
        }

        BankAccountModel targetBankAccountModel = null;
        if (vehicle.DbEntity.GroupModelOwnerId.HasValue)
        {
            targetBankAccountModel =
                await _bankAccountService.GetByOwningGroup(vehicle.DbEntity.GroupModelOwnerId.Value);
        }
        else
        {
            targetBankAccountModel = await _bankAccountService.GetByKey(targetBankAccountId);
        }

        if (targetBankAccountModel == null)
        {
            player.SendNotification("Das Bankkonto von der anderen Person existiert nicht mehr.",
                                    NotificationType.ERROR);
            seller.SendNotification("Kauf konnte nicht abgeschlossen werden.", NotificationType.ERROR);
            return;
        }

        if (!await _inventoryModule.CanCarry(player, ItemCatalogIds.KEY))
        {
            seller.SendNotification("Kauf konnte nicht abgeschlossen werden.", NotificationType.ERROR);
            return;
        }

        var success = await _bankModule.Withdraw(bankAccount, price, false, "Fahrzeugkauf");
        if (success)
        {
            await _bankModule.Deposit(targetBankAccountModel, price, "Fahrzeugverkauf");
            await _vehicleModule.SetCharacterOwner(vehicle, player);

            player.SendNotification("Kauf erfolgreich abgeschlossen.", NotificationType.SUCCESS);
            seller.SendNotification("Kauf erfolgreich abgeschlossen.", NotificationType.SUCCESS);
        }
        else
        {
            player.SendNotification("Dein Charakter hat nicht Geld auf dem Bankkonto.", NotificationType.ERROR);
            seller.SendNotification("Kauf konnte nicht abgeschlossen werden.", NotificationType.ERROR);
        }
    }

    private static ServerVehicle? GetVehicleCheckIfSellerIsOwner(ServerPlayer player, ServerPlayer seller)
    {
        if (!seller.Exists)
        {
            player.SendNotification("Es wurde kein Spieler gefunden.", NotificationType.ERROR);
            return null;
        }

        if (!seller.HasData("VEHICLE_SELL_VEHICLE_ID"))
        {
            player.SendNotification("Es wurde kein Fahrzeug gefunden.", NotificationType.ERROR);
            return null;
        }

        seller.GetData("VEHICLE_SELL_VEHICLE_ID", out int vehicleDbId);
        seller.DeleteData("VEHICLE_SELL_VEHICLE_ID");

        var vehicle = Alt.GetAllVehicles().FindByDbId(vehicleDbId);
        if (vehicle is not { Exists: true })
        {
            player.SendNotification("Es wurde kein Fahrzeug gefunden.", NotificationType.ERROR);
            return null;
        }

        var isGroupVehicle = vehicle.DbEntity.GroupModelOwnerId.HasValue;
        if (isGroupVehicle)
        {
            seller.GetData("VEHICLE_SELL_GROUP_ID", out int groupId);
            seller.DeleteData("VEHICLE_SELL_GROUP_ID");
            if (vehicle.DbEntity.GroupModelOwnerId != groupId)
            {
                seller.SendNotification("Die Gruppe ist nicht mehr der Besitzer dieses Fahrzeuges.",
                                        NotificationType.ERROR);
                player.SendNotification("Der Verkäufer ist nicht mehr der Besitzer dieses Fahrzeuges.",
                                        NotificationType.ERROR);
                return null;
            }
        }
        else
        {
            seller.GetData("VEHICLE_SELL_OWNER_ID", out int ownerId);
            seller.DeleteData("VEHICLE_SELL_OWNER_ID");
            if (vehicle.DbEntity.CharacterModelId != ownerId)
            {
                seller.SendNotification("Du bist nicht mehr der Besitzer des Fahrzeuges.",
                                        NotificationType.ERROR);
                player.SendNotification("Der Verkäufer ist nicht mehr der Besitzer dieses Fahrzeuges.",
                                        NotificationType.ERROR);
                return null;
            }
        }

        return vehicle;
    }
}