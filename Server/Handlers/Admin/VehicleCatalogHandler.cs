using System;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Vehicles;
using Server.Helper;
using Server.Modules.Admin;
using Server.Modules.Dump;
using Server.Modules.SouthCentralPoints;

namespace Server.Handlers.Admin;

public class VehicleCatalogHandler : ISingletonScript
{
    private readonly AdminModule _adminModule;
    private readonly Serializer _serializer;
    private readonly SouthCentralPointsModule _southCentralPointsModule;
    private readonly VehicleCatalogService _vehicleCatalogService;
    private readonly VehicleDumpModule _vehicleDumpModule;

    public VehicleCatalogHandler(Serializer serializer, AdminModule adminModule,
        SouthCentralPointsModule southCentralPointsModule, VehicleDumpModule vehicleDumpModule,
        VehicleCatalogService vehicleCatalogService)
    {
        _serializer = serializer;
        _adminModule = adminModule;
        _southCentralPointsModule = southCentralPointsModule;
        _vehicleDumpModule = vehicleDumpModule;
        _vehicleCatalogService = vehicleCatalogService;

        AltAsync.OnClient<ServerPlayer>("vehiclecatalog:open", OnOpenVehicleCatalog);
        AltAsync.OnClient<ServerPlayer, string, int>("vehiclecatalog:requestsetorderablevehiclesamount",
            OnRequestSetOrderableVehicleAmount);
        AltAsync.OnClient<ServerPlayer, string>("vehiclecatalog:saveveh", OnSaveVeh);
    }

    private async void OnOpenVehicleCatalog(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.AccountModel.Permission.HasFlag(Permission.ECONOMY_MANAGEMENT))
        {
            player.SendNotification("Du hast nicht genug Berechtigungen.", NotificationType.ERROR);
            return;
        }

        var catalogVehicles = await _vehicleCatalogService.GetAll();
        catalogVehicles.ForEach(v => v.SouthCentralPoints = _southCentralPointsModule.GetPointsPrice(v.Price));
        player.EmitGui("vehiclecatalog:open", catalogVehicles);
    }

    private async void OnRequestSetOrderableVehicleAmount(ServerPlayer player, string model,
        int amountOfOrderableVehicles)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.AccountModel.Permission.HasFlag(Permission.HEAD_ECONOMY_MANAGEMENT))
        {
            player.SendNotification("Du hast nicht genug Berechtigungen.", NotificationType.ERROR);
            return;
        }

        var catalogVehicle = await _vehicleCatalogService.GetByKey(model);
        if (catalogVehicle == null)
        {
            return;
        }

        if (catalogVehicle.Price == 0)
        {
            player.SendNotification("Dieses Fahrzeug hat kein Preis, du kannst es nicht beim Händler anbieten.",
                NotificationType.ERROR);
            return;
        }

        catalogVehicle.AmountOfOrderableVehicles = amountOfOrderableVehicles;

        await _vehicleCatalogService.Update(catalogVehicle);

        player.SendNotification(
            "Du hast die Anzahl der bestellbaren Fahrzeuge für das " + $"Fahrzeug: {catalogVehicle.DisplayName} auf " +
            $"{amountOfOrderableVehicles} Stück gestellt.", NotificationType.SUCCESS);

        UpdateVehicleCatalogUi(catalogVehicle);
    }


    private async void OnSaveVeh(ServerPlayer player, string catalogVehicleJson)
    {
        var catalogVehicle = _serializer.Deserialize<CatalogVehicleModel>(catalogVehicleJson);

        var dump = _vehicleDumpModule.Dump.Find(v =>
            string.Equals(v.Name, catalogVehicle.Model, StringComparison.CurrentCultureIgnoreCase));

        catalogVehicle.ClassId = dump == null ? "NOT_FOUND" : dump.Class;
        catalogVehicle.DlcName = dump == null ? "NOT_FOUND" : dump.DlcName;

        await _vehicleCatalogService.Add(catalogVehicle);

        player.SendNotification("Fahrzeug wurde dem Katalog hinzugefügt.", NotificationType.SUCCESS);
    }

    private void UpdateVehicleCatalogUi(CatalogVehicleModel catalogVehicleModel)
    {
        var callback = new Action<ServerPlayer>(player =>
        {
            catalogVehicleModel.SouthCentralPoints =
                _southCentralPointsModule.GetPointsPrice(catalogVehicleModel.Price);

            player.EmitGui("vehiclecatalog:update", catalogVehicleModel);
        });

        _adminModule.GetAllStaffPlayers().ForEach(callback);
    }
}