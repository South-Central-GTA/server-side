using System;
using System.Linq;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Callbacks;
using Server.Core.Configuration;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Delivery;
using Server.Database.Models.Group;
using Server.Modules.Bank;
using Server.Modules.Vehicles;

namespace Server.Modules.Delivery;

public class DeliveryModule : ITransientScript
{
    private readonly BankAccountService _bankAccountService;
    private readonly BankModule _bankModule;
    private readonly DeliveryOptions _deliveryOptions;

    private readonly DeliveryService _deliveryService;
    private readonly GroupService _groupService;
    private readonly HouseService _houseService;
    private readonly ILogger<DeliveryModule> _logger;
    private readonly Random _random = new();
    private readonly VehicleCatalogService _vehicleCatalogService;
    private readonly VehicleModule _vehicleModule;

    private readonly VehicleSelectionModule _vehicleSelectionModule;
    private readonly WorldLocationOptions _worldLocationOptions;

    public DeliveryModule(ILogger<DeliveryModule> logger, IOptions<WorldLocationOptions> worldLocationOptions,
        IOptions<DeliveryOptions> deliveryOptions, VehicleSelectionModule vehicleSelectionModule, BankModule bankModule,
        VehicleModule vehicleModule, DeliveryService deliveryService, HouseService houseService,
        GroupService groupService, BankAccountService bankAccountService, VehicleCatalogService vehicleCatalogService)
    {
        _logger = logger;
        _worldLocationOptions = worldLocationOptions.Value;
        _deliveryOptions = deliveryOptions.Value;

        _vehicleSelectionModule = vehicleSelectionModule;
        _bankModule = bankModule;
        _vehicleModule = vehicleModule;

        _deliveryService = deliveryService;
        _houseService = houseService;
        _groupService = groupService;
        _bankAccountService = bankAccountService;
        _vehicleCatalogService = vehicleCatalogService;
    }

    public async Task UpdateGroupDeliveriesUi(ServerPlayer player, int companyId)
    {
        var deliveries = await _deliveryService.Where(d => d.OrderGroupModelId == companyId);

        player.EmitGui("delivery:sendgroupdeliveries", deliveries);
    }

    public async Task UpdateOpenDeliveriesUi()
    {
        var callback = new AsyncFunctionCallback<IPlayer>(async player =>
        {
            if (!player.Exists)
            {
                return;
            }

            var serverPlayer = (ServerPlayer)player;

            if (!serverPlayer.IsSpawned)
            {
                return;
            }

            await UpdatePlayerOpenDeliveriesUi(serverPlayer);

            await Task.CompletedTask;
        });

        await Alt.ForEachPlayers(callback);
    }

    public void UpdateGroupOpenDeliveriesUi(GroupModel groupModel)
    {
        var groupPlayers = Alt.GetAllPlayers().Where(p =>
            p.Exists && p.IsSpawned && groupModel.Members.Any(m => m.CharacterModelId == p.CharacterModel.Id));

        var callback = new Action<ServerPlayer>(async player => { await UpdatePlayerOpenDeliveriesUi(player); });

        groupPlayers.ForEach(callback);
    }

    public async Task UpdatePlayerOpenDeliveriesUi(ServerPlayer player)
    {
        var groups = await _groupService.GetGroupsByCharacter(player.CharacterModel.Id);
        if (groups == null)
        {
            return;
        }

        var deliveries = await _deliveryService.Where(d => d.Status == DeliveryState.OPEN);
        deliveries = deliveries.FindAll(d =>
            ((CompanyGroupModel)d.OrderGroupModel).DeliveryVisibilityStatus == VisiblityState.PUBLIC ||
            groups.Any(g => g.Id == d.OrderGroupModelId));

        player.EmitGui("delivery:sendopendeliveries", deliveries);
    }


    public async Task CollectDelivery(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        if (player.Position.Distance(new Position(_worldLocationOptions.HarbourSelectionPositionX,
                _worldLocationOptions.HarbourSelectionPositionY, _worldLocationOptions.HarbourSelectionPositionZ)) > 5)
        {
            player.SendNotification("Dein Charakter ist nicht in der Nähe einer Aufladestation.",
                NotificationType.ERROR);
            return;
        }

        if (!player.IsInVehicle)
        {
            player.SendNotification("Dein Charakter muss in einem Fahrzeug sitzen.", NotificationType.ERROR);
            return;
        }

        var groups = await _groupService.GetGroupsByCharacter(player.CharacterModel.Id);
        var group = groups?.FirstOrDefault(g => g.GroupType == GroupType.COMPANY);
        if (group == null)
        {
            player.SendNotification("Dein Charakter ist in keinem Unternehmen.", NotificationType.ERROR);
            return;
        }

        var companyGroup = (CompanyGroupModel)group;

        if (!companyGroup.LicensesFlags.HasFlag(LicensesFlags.GOODS_TRANSPORT))
        {
            player.SendNotification("Das Unternehmen hat keine Speditionslizenz.", NotificationType.ERROR);
            return;
        }

        var delivery = await _deliveryService.Find(d => d.SupplierCharacterId == player.CharacterModel.Id);
        if (delivery == null)
        {
            player.SendNotification("Dein Charakter hat kein Lieferauftrag.", NotificationType.ERROR);
            return;
        }

        var vehicle = (ServerVehicle)(player.Vehicle.Attached ?? player.Vehicle);

        if (vehicle is not { Exists: true })
        {
            return;
        }

        if (vehicle.DbEntity == null)
        {
            player.SendNotification("Mit gespawnten Fahrzeugen kannst du keine Aufträge fahren.",
                NotificationType.ERROR);
            return;
        }

        switch (delivery.DeliveryType)
        {
            case DeliveryType.DELIVERY:
                break;
            case DeliveryType.PRODUCT:
                delivery = await HandleCollectProductDelivery(player, vehicle, (ProductDeliveryModel)delivery);
                break;
            case DeliveryType.LIQUID:
                break;
            case DeliveryType.VEHICLES:
                delivery = await HandleCollectVehicleDelivery(player, vehicle, (VehicleDeliveryModel)delivery);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (delivery == null)
        {
            return;
        }

        delivery.Status = DeliveryState.LOADED;
        delivery.PlayerVehicleModelId = vehicle.DbEntity.Id;

        await _deliveryService.Update(delivery);

        var house = await _houseService.Find(h => h.GroupModelId == delivery.OrderGroupModelId);

        player.SetWaypoint(new Position(house.PositionX, house.PositionY, house.PositionZ), 5, 1);

        player.EmitGui("delivery:sendmycurrentdelivery", delivery);

        UpdateGroupOpenDeliveriesUi(delivery.OrderGroupModel);

        player.SendNotification(
            "Es ist verboten Roleplay zu rushen, spiele das Verladen sowie aber noch viel wichtiger das Abladen korrekt aus, andere Spieler könnten dich sehen.",
            NotificationType.INFO);
        player.SendNotification(
            "Waren wurden erfolgreich aufgeladen, fahre nun zum markierten Ziel und gebe die Waren mit /deploy ab.",
            NotificationType.SUCCESS);
    }

    private async Task<DeliveryModel?> HandleCollectProductDelivery(ServerPlayer player, ServerVehicle vehicle,
        ProductDeliveryModel productDeliveryModel)
    {
        var maxLoad =
            _vehicleSelectionModule.GetVehicleTransportSize(((VehicleModel)vehicle.Model).ToString().ToLower());
        if (!maxLoad.HasValue)
        {
            player.SendNotification("Mit diesem Fahrzeug können keine Lieferungen getätigt werden.",
                NotificationType.ERROR);
            return null;
        }

        if (maxLoad < productDeliveryModel.ProductsRemaining)
        {
            player.SendNotification(
                $"Es passen nicht alle bestellten Produkte in das Fahrzeug, es wurden {maxLoad} Produkte aufgeladen.",
                NotificationType.WARNING);
        }

        if (productDeliveryModel.ProductsRemaining < maxLoad.Value)
        {
            maxLoad = productDeliveryModel.ProductsRemaining;
            productDeliveryModel.ProductsRemaining = 0;
        }
        else
        {
            productDeliveryModel.ProductsRemaining -= maxLoad.Value;
        }

        lock (vehicle)
        {
            vehicle.SetData("AMOUNT_OF_PRODUCTS", maxLoad);
        }

        return productDeliveryModel;
    }

    private async Task<DeliveryModel?> HandleCollectVehicleDelivery(ServerPlayer player, ServerVehicle vehicle,
        VehicleDeliveryModel vehicleDeliveryModel)
    {
        if ((VehicleModel)vehicle.Model != VehicleModel.Tr2)
        {
            player.SendNotification("Mit diesem Fahrzeug können keine Fahrezuglieferungen getätigt werden.",
                NotificationType.ERROR);
            return null;
        }

        lock (vehicle)
        {
            if (vehicle.HasData("AMOUNT_OF_VEHICLES") &&
                vehicle.GetData("AMOUNT_OF_VEHICLES", out int currentLoadedAmount))
            {
                if (currentLoadedAmount >= 6)
                {
                    player.SendNotification("Auf den Anhänger bekommst du nicht mehr als 6 Fahrzeuge.",
                        NotificationType.ERROR);

                    return vehicleDeliveryModel;
                }
            }

            var amountOfVehicles = 1;

            if (vehicle.HasData("AMOUNT_OF_VEHICLES"))
            {
                vehicle.GetData("AMOUNT_OF_VEHICLES", out int amount);
                amountOfVehicles += amount;
            }

            vehicle.SetData("AMOUNT_OF_VEHICLES", amountOfVehicles);
        }

        return vehicleDeliveryModel;
    }


    public async Task DeployDelivery(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        if (player.IsInVehicle)
        {
            var trailerOrMainVeh = player.Vehicle.Attached ?? player.Vehicle;
            if (!trailerOrMainVeh.HasData("AMOUNT_OF_PRODUCTS") && !trailerOrMainVeh.HasData("AMOUNT_OF_VEHICLES"))
            {
                player.SendNotification("Das Fahrzeug ist nicht beladen.", NotificationType.ERROR);
                return;
            }
        }
        else
        {
            player.SendNotification("Dein Charakter muss in einem Fahrzeug sitzen.", NotificationType.ERROR);
            return;
        }

        var groups = await _groupService.GetGroupsByCharacter(player.CharacterModel.Id);
        var group = groups?.FirstOrDefault(g => g.GroupType == GroupType.COMPANY);
        if (group == null)
        {
            player.SendNotification("Dein Charakter ist in keinem Unternehmen.", NotificationType.ERROR);
            return;
        }

        var companyGroup = (CompanyGroupModel)group;

        if (!companyGroup.LicensesFlags.HasFlag(LicensesFlags.GOODS_TRANSPORT))
        {
            player.SendNotification("Das Unternehmen hat keine Speditionslizenz.", NotificationType.ERROR);
            return;
        }

        var deliveries = await _deliveryService.Where(d => d.SupplierCharacterId == player.CharacterModel.Id);
        if (deliveries == null)
        {
            player.SendNotification("Dein Charakter hat keine Lieferaufträge.", NotificationType.ERROR);
            return;
        }

        foreach (var t in deliveries)
        {
            var delivery = t;
            var house = await _houseService.Find(h => h.GroupModelId == delivery.OrderGroupModelId);

            if (player.Position.Distance(new Position(house.PositionX, house.PositionY, house.PositionZ)) > 30)
            {
                player.SendNotification("Dein Charakter ist nicht in der Nähe eines Zieles.", NotificationType.ERROR);
                return;
            }

            var vehicle = player.Vehicle.Attached ?? player.Vehicle;

            switch (delivery.DeliveryType)
            {
                case DeliveryType.DELIVERY:
                    break;
                case DeliveryType.PRODUCT:
                    delivery = await HandleDeployProductDelivery(player, (ProductDeliveryModel)delivery, vehicle);
                    break;
                case DeliveryType.LIQUID:
                    break;
                case DeliveryType.VEHICLES:
                    delivery = await HandleDeployVehicleDelivery(player, (VehicleDeliveryModel)delivery, vehicle);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (delivery == null)
            {
                return;
            }

            await _deliveryService.Update(delivery);

            await UpdateOpenDeliveriesUi();
            UpdateGroupOpenDeliveriesUi(delivery.OrderGroupModel);
        }
    }

    private async Task<DeliveryModel?> HandleDeployProductDelivery(ServerPlayer player,
        ProductDeliveryModel deliveryModel, IVehicle vehicle)
    {
        if (!vehicle.HasData("AMOUNT_OF_PRODUCTS"))
        {
            player.SendNotification("Fahrzeug oder Anhänger hat keine Produkte beladen.", NotificationType.ERROR);
            return null;
        }

        vehicle.GetData("AMOUNT_OF_PRODUCTS", out int amountOfProducts);

        if (deliveryModel.OrderGroupModel is CompanyGroupModel companyGroup)
        {
            companyGroup.Products += amountOfProducts;
            await _groupService.Update(companyGroup);
        }

        var bankAccount = await _bankAccountService.GetByOwningGroup(deliveryModel.OrderGroupModelId);
        if (bankAccount == null)
        {
            player.SendNotification(
                "Das Bankkonto des Unternehmens existiert nicht mehr, der Auftrag kann nicht beendet werden.",
                NotificationType.ERROR);
            return null;
        }

        var price = amountOfProducts * _deliveryOptions.ProductPrice;
        await _bankModule.Withdraw(bankAccount, price, true, "Produktlieferung");

        var supplierGroupBankAccount = await _bankAccountService.GetByOwningGroup(deliveryModel.OrderGroupModel.Id);
        if (supplierGroupBankAccount == null)
        {
            player.SendNotification(
                "Das Bankkonto deines Unternehmens existiert nicht mehr, der Auftrag kann nicht beendet werden.",
                NotificationType.ERROR);
            return null;
        }

        var salaryForSupplierGroup =
            (int)(amountOfProducts * _deliveryOptions.ProductPrice * _deliveryOptions.SharesForSuppliers);
        await _bankModule.Deposit(supplierGroupBankAccount, salaryForSupplierGroup, "Produktlieferung");

        vehicle.DeleteData("AMOUNT_OF_PRODUCTS");

        if (deliveryModel.ProductsRemaining <= 0)
        {
            deliveryModel.Status = DeliveryState.DELIVERD;
            deliveryModel.SupplierCharacterId = null;
            deliveryModel.PlayerVehicleModelId = null;

            player.EmitGui("delivery:stopmydelivery");
            player.ClearWaypoint();

            player.SendNotification(
                "Waren erfolgreich abgeladen und Auftrag abgeschlossen. Das Transportunternehmen, erhält das Geld auf das Unternehmenskonto überwiesen.",
                NotificationType.SUCCESS);
        }
        else
        {
            player.SendNotification(
                $"Der Lieferauftrag ist noch nicht abgeschlossen. Es konnten nur {amountOfProducts} aufgeladen werden, es fehlen noch {deliveryModel.ProductsRemaining} Produkte.",
                NotificationType.WARNING);

            player.SendNotification(
                "Waren erfolgreich abgeladen, Das Transportunternehmen erhält das Geld auf das Unternehmenskonto überwiesen.",
                NotificationType.SUCCESS);

            deliveryModel.Status = DeliveryState.DELIVERD_BACK_TO_HARBOR;
        }

        return deliveryModel;
    }

    private async Task<DeliveryModel?> HandleDeployVehicleDelivery(ServerPlayer player,
        VehicleDeliveryModel deliveryModel, IVehicle vehicle)
    {
        if (!vehicle.HasData("AMOUNT_OF_VEHICLES"))
        {
            player.SendNotification("Fahrzeug oder Anhänger hat keine Fahrzeuge beladen.", NotificationType.ERROR);
            return null;
        }

        var bankAccount = await _bankAccountService.GetByOwningGroup(deliveryModel.OrderGroupModelId);
        if (bankAccount == null)
        {
            player.SendNotification(
                "Das Bankkonto des Unternehmens existiert nicht mehr, der Auftrag kann nicht beendet werden.",
                NotificationType.ERROR);
            return null;
        }

        var catalogVehicle = await _vehicleCatalogService.GetByKey(deliveryModel.VehicleModel);
        if (catalogVehicle == null)
        {
            player.SendNotification(
                "Du Lieferung konnte nicht bearbeitet werden da dieses Fahrzeug nicht " +
                "in unserem Katalog definiert ist, der Auftrag kann nicht beendet werden. Melde dich bitte bei einem Administrator.",
                NotificationType.ERROR);
            return null;
        }

        var price = (int)(catalogVehicle.Price * 0.01f);
        await _bankModule.Withdraw(bankAccount, price, true, $"Fahrzeuglieferung: '{catalogVehicle.DisplayName}'");

        var supplierGroupBankAccount = await _bankAccountService.GetByOwningGroup(deliveryModel.OrderGroupModel.Id);
        if (supplierGroupBankAccount == null)
        {
            player.SendNotification(
                "Das Bankkonto deines Unternehmens existiert nicht mehr, der Auftrag kann nicht beendet werden.",
                NotificationType.ERROR);
            return null;
        }

        var salaryForSupplierGroup = (int)(price * _deliveryOptions.SharesForSuppliers);
        await _bankModule.Deposit(supplierGroupBankAccount, salaryForSupplierGroup,
            $"Fahrzeuglieferung: '{catalogVehicle.DisplayName}'");

        if (vehicle.HasData("AMOUNT_OF_VEHICLES"))
        {
            vehicle.GetData("AMOUNT_OF_VEHICLES", out int amount);
            if (amount > 1)
            {
                amount--;
                vehicle.SetData("AMOUNT_OF_VEHICLES", amount);
            }
            else
            {
                vehicle.DeleteData("AMOUNT_OF_VEHICLES");
            }
        }

        var randomColor = _random.Next(0, 111);

        await _vehicleModule.CreatePersistent(deliveryModel.VehicleModel, deliveryModel.OrderGroupModel, Position.Zero,
            Rotation.Zero, 0, randomColor, randomColor, VehicleState.IN_STORAGE, _random.Next(60000, 80000));

        deliveryModel.Status = DeliveryState.DELIVERD;
        deliveryModel.SupplierCharacterId = null;
        deliveryModel.PlayerVehicleModelId = null;

        player.EmitGui("delivery:stopmydelivery");
        player.ClearWaypoint();

        player.SendNotification(
            "Fahrzeug erfolgreich abgeladen und ins Lager gebracht. Das Transportunternehmen, erhält das Geld auf das Unternehmenskonto überwiesen.",
            NotificationType.SUCCESS);

        return deliveryModel;
    }
}