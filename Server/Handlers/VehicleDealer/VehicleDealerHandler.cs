using System;
using System.Linq;
using AltV.Net;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Delivery;
using Server.Database.Models.Group;
using Server.Database.Models.Vehicles;
using Server.Modules.Bank;
using Server.Modules.Delivery;
using Server.Modules.Group;

namespace Server.Handlers.VehicleDealer;

public class VehicleDealerHandler : ISingletonScript
{
    private readonly BankAccountService _bankAccountService;

    private readonly BankModule _bankModule;
    private readonly DeliveryModule _deliveryModule;
    private readonly DeliveryService _deliveryService;
    private readonly GroupModule _groupModule;

    private readonly GroupService _groupService;
    private readonly OrderedVehicleService _orderedVehicleService;
    private readonly Random _random = new();
    private readonly VehicleCatalogService _vehicleCatalogService;

    public VehicleDealerHandler(
        GroupService groupService,
        VehicleCatalogService vehicleCatalogService,
        BankAccountService bankAccountService,
        OrderedVehicleService orderedVehicleService,
        DeliveryService deliveryService,
        BankModule bankModule,
        DeliveryModule deliveryModule,
        GroupModule groupModule)
    {
        _groupService = groupService;
        _vehicleCatalogService = vehicleCatalogService;
        _bankAccountService = bankAccountService;
        _orderedVehicleService = orderedVehicleService;
        _deliveryService = deliveryService;

        _bankModule = bankModule;
        _deliveryModule = deliveryModule;
        _groupModule = groupModule;

        AltAsync.OnClient<ServerPlayer>("vehicledealer:requestpage", OnRequestPage);
        AltAsync.OnClient<ServerPlayer, string>("vehicledealer:reqeustorder", OnRequestOrder);
        AltAsync.OnClient<ServerPlayer>("vehicledealer:getorders", OnGetOrders);
        AltAsync.OnClient<ServerPlayer, int>("vehicledealer:requestdelivery", OnRequestDelivery);
    }

    private async void OnRequestPage(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        var hasAccess = false;
        var groups = await _groupService.GetGroupsByCharacter(player.CharacterModel.Id);
        var group = groups?.FirstOrDefault(g => g.GroupType == GroupType.COMPANY);
        if (group is CompanyGroupModel companyGroup)
        {
            if (companyGroup.LicensesFlags.HasFlag(LicensesFlags.VEHICLE_DEALERSHIP))
            {
                hasAccess = await _groupModule.HasPermission(player.CharacterModel.Id,
                                                             companyGroup.Id,
                                                             GroupPermission.ORDER_VEHICLES);
            }
        }

        var vehicles = await _vehicleCatalogService.Where(cv => cv.AmountOfOrderableVehicles > 0 && cv.Price != 0);

        player.EmitGui("vehicledealer:setup", player.CharacterModel.Name, hasAccess, vehicles);
    }

    private async void OnRequestOrder(ServerPlayer player, string vehicleModel)
    {
        if (!player.Exists)
        {
            return;
        }

        var groups = await _groupService.GetGroupsByCharacter(player.CharacterModel.Id);
        var group = groups?.FirstOrDefault(g => g.GroupType == GroupType.COMPANY);
        if (group == null)
        {
            return;
        }

        var companyGroup = (CompanyGroupModel)group;

        if (!companyGroup.LicensesFlags.HasFlag(LicensesFlags.VEHICLE_DEALERSHIP))
        {
            return;
        }

        var vehicle = await _vehicleCatalogService.GetByKey(vehicleModel);
        if (vehicle == null)
        {
            return;
        }

        if (vehicle.AmountOfOrderableVehicles <= 0)
        {
            player.EmitGui("vehicledealer:setwarning",
                           "Leider ist das Kontingent für diesen Fahrzeugtypen erschöpft, wir haben kein Fahrzeug mehr auf Lager.");
            return;
        }

        var bankAccount = await _bankAccountService.Find(ba =>
                                                             ba.Type == OwnableAccountType.GROUP
                                                             && ba.GroupRankAccess.Any(
                                                                 gra => gra.GroupModelId == companyGroup.Id));

        if (!await _groupModule.HasPermission(player.CharacterModel.Id, group.Id, GroupPermission.ORDER_VEHICLES))
        {
            player.SendNotification("Dein Charakter hat dafür nicht genügend Berechtigungen in der Gruppe.",
                                    NotificationType.ERROR);
            return;
        }

        var success = await _bankModule.Withdraw(bankAccount,
                                                 vehicle.Price,
                                                 false,
                                                 $"Fahrzeugbestellung '{vehicle.DisplayName}'");
        if (!success)
        {
            player.EmitGui("vehicledealer:setwarning",
                           "Auf dem hinterlegten Unternehmenskonto liegt nicht genügend Geld zur Verfügung für diesen Kauf.");
            return;
        }

        vehicle.AmountOfOrderableVehicles--;
        await _vehicleCatalogService.Update(vehicle);

        await _orderedVehicleService.Add(new OrderedVehicleModel
        {
            GroupModelId = group.Id,
            CatalogVehicleModelId = vehicle.Model,
            OrderedBy = player.CharacterModel.Name,
            DeliverdAt = DateTime.Now.AddMinutes(2).AddSeconds(-DateTime.Now.Second)
            // DeliverdAt = DateTime.Now.AddDays(_random.Next(1, 4)) // TODO: Activate when going live
        });

        player.EmitGui("vehicledealer:ordersuccessfull");

        UpdateOrderableVehiclesUi(vehicle);
        UpdateGroupOrdersUi(group);
    }

    private async void OnGetOrders(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        var groups = await _groupService.GetGroupsByCharacter(player.CharacterModel.Id);
        var group = groups?.FirstOrDefault(g => g.GroupType == GroupType.COMPANY);
        if (group == null)
        {
            return;
        }

        var companyGroup = (CompanyGroupModel)group;

        if (!companyGroup.LicensesFlags.HasFlag(LicensesFlags.VEHICLE_DEALERSHIP))
        {
            return;
        }

        var orderedVehicles = await _orderedVehicleService.Where(ov => ov.GroupModelId == group.Id);
        player.EmitGui("vehicledealer:setorders", orderedVehicles);
    }

    private async void OnRequestDelivery(ServerPlayer player, int orderedVehicleId)
    {
        if (!player.Exists)
        {
            return;
        }

        var groups = await _groupService.GetGroupsByCharacter(player.CharacterModel.Id);
        var group = groups?.FirstOrDefault(g => g.GroupType == GroupType.COMPANY);
        if (group == null)
        {
            return;
        }

        var companyGroup = (CompanyGroupModel)group;

        if (!companyGroup.LicensesFlags.HasFlag(LicensesFlags.VEHICLE_DEALERSHIP))
        {
            return;
        }

        if (!await _groupModule.HasPermission(player.CharacterModel.Id, group.Id, GroupPermission.ORDER_VEHICLES))
        {
            player.SendNotification("Dein Charakter hat dafür nicht genügend Berechtigungen in der Gruppe.",
                                    NotificationType.ERROR);
            return;
        }

        var orderedVehicles = await _orderedVehicleService.Where(ov => ov.GroupModelId == group.Id);
        var orderedVehicle = orderedVehicles.Find(ov => ov.Id == orderedVehicleId);
        if (orderedVehicle?.DeliveryRequestedBy == null)
        {
            return;
        }

        orderedVehicle.DeliveryRequestedAt = DateTime.Now;
        orderedVehicle.DeliveryRequestedBy = player.CharacterModel.Name;

        await _orderedVehicleService.Update(orderedVehicle);

        await _deliveryService.Add(new VehicleDeliveryModel(companyGroup.Id,
                                                            orderedVehicle.CatalogVehicleModel.Model,
                                                            orderedVehicle.CatalogVehicleModel.DisplayName));

        await _deliveryModule.UpdateOpenDeliveriesUi();

        UpdateGroupOrdersUi(group);
    }

    private void UpdateGroupOrdersUi(GroupModel groupModel)
    {
        var groupPlayers = Alt.GetAllPlayers()
                              .Where(p => p.Exists && p.IsSpawned && groupModel.Members
                                                                               .Any(m => m.CharacterModelId ==
                                                                                         p.CharacterModel.Id));

        var callback = new Action<ServerPlayer>(async player =>
        {
            if (await _groupModule.HasPermission(player.CharacterModel.Id,
                                                 groupModel.Id,
                                                 GroupPermission.ORDER_VEHICLES))
            {
                var orderedVehicles = await _orderedVehicleService.Where(ov => ov.GroupModelId == groupModel.Id);
                player.EmitGui("vehicledealer:setorders", orderedVehicles);
            }
        });

        groupPlayers.ForEach(callback);
    }

    private void UpdateOrderableVehiclesUi(CatalogVehicleModel catalogVehicleModel)
    {
        var callback = new Action<ServerPlayer>(player =>
        {
            player.EmitGui("vehicledealer:update", catalogVehicleModel);
        });

        Alt.GetAllPlayers().Where(p => p.Exists && p.IsSpawned).ForEach(callback);
    }
}