using System.Collections.Generic;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;

namespace Server.Handlers.Admin;

public class GroupCatalogHandler : ISingletonScript
{
    private readonly BankAccountService _bankAccountService;
    private readonly GroupService _groupService;
    private readonly VehicleCatalogService _vehicleCatalogService;
    private readonly VehicleService _vehicleService;

    public GroupCatalogHandler(
        BankAccountService bankAccountService,
        GroupService groupService,
        VehicleCatalogService vehicleCatalogService,
        VehicleService vehicleService)
    {
        _bankAccountService = bankAccountService;
        _groupService = groupService;
        _vehicleCatalogService = vehicleCatalogService;
        _vehicleService = vehicleService;
        AltAsync.OnClient<ServerPlayer>("groupcatalog:open", OnOpenGroupCatalog);
        AltAsync.OnClient<ServerPlayer, int>("groupcatalog:requestdetails", OnRequestDetails);
    }

    private async void OnOpenGroupCatalog(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.AccountModel.Permission.HasFlag(Permission.STAFF))
        {
            return;
        }

        player.EmitGui("groupcatalog:setup", await _groupService.GetAll());
    }

    private async void OnRequestDetails(ServerPlayer player, int groupId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.AccountModel.Permission.HasFlag(Permission.STAFF))
        {
            return;
        }

        var group = await _groupService.GetByKey(groupId);
        var bankAccount = await _bankAccountService.GetByGroup(groupId);
        var vehicles = await _vehicleService.Where(v => v.GroupModelOwnerId == groupId);
        var vehicleDatas = new List<VehicleData>();

        foreach (var vehicle in vehicles)
        {
            var catalogVehicle = await _vehicleCatalogService.GetByKey(vehicle.Model.ToLower());
            if (catalogVehicle == null)
            {
                continue;
            }

            vehicleDatas.Add(new VehicleData
            {
                Id = vehicle.Id,
                DisplayName = catalogVehicle.DisplayName,
                DisplayClass = catalogVehicle.DisplayClass
            });
        }

        player.EmitGui("groupcatalog:opendetails", group, bankAccount, vehicleDatas);
    }
}