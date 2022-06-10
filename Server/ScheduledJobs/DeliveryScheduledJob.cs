using System;
using System.Threading.Tasks;
using AltV.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Server.Core.Configuration;
using Server.Core.Extensions;
using Server.Core.ScheduledJobs;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Delivery;
using Server.Modules.Delivery;

namespace Server.ScheduledJobs;

public class DeliveryScheduledJob : ScheduledJob
{
    private readonly DeliveryModule _deliveryModule;
    private readonly DeliveryOptions _deliveryOptions;

    private readonly DeliveryService _deliveryService;
    private readonly ILogger<DeliveryScheduledJob> _logger;

    public DeliveryScheduledJob(ILogger<DeliveryScheduledJob> logger, IOptions<DeliveryOptions> deliveryOptions,
        DeliveryService deliveryService, DeliveryModule deliveryModule) : base(TimeSpan.FromMinutes(1))
    {
        _logger = logger;
        _deliveryOptions = deliveryOptions.Value;

        _deliveryService = deliveryService;

        _deliveryModule = deliveryModule;
    }

    public override async Task Action()
    {
        var acceptedDeliveries = await _deliveryService.Where(d => d.Status == DeliveryState.ACCEPTED);
        acceptedDeliveries.FindAll(d => (DateTime.Now - d.LastUsage).TotalMinutes >= _deliveryOptions.PickupTime - 5)
            .ForEach(d =>
            {
                if (!d.SupplierCharacterId.HasValue)
                {
                    return;
                }

                var player = Alt.GetAllPlayers().FindPlayerByCharacterId(d.SupplierCharacterId.Value);
                d = ClearProductDelivery(d);

                player?.SendNotification(
                    "Deine Lieferung ist in 5 Minuten fehlgeschlagen da du sie nicht rechtzeitig am Hafen aufgeladen hast.",
                    NotificationType.WARNING);
            });

        acceptedDeliveries.FindAll(d => (DateTime.Now - d.LastUsage).TotalMinutes >= _deliveryOptions.PickupTime)
            .ForEach(d =>
            {
                if (!d.SupplierCharacterId.HasValue)
                {
                    return;
                }

                var player = Alt.GetAllPlayers().FindPlayerByCharacterId(d.SupplierCharacterId.Value);
                d = ClearProductDelivery(d);

                if (player != null)
                {
                    player.SendNotification(
                        "Dein Charakter hat es nicht geschafft rechtzeitig die Lieferung am Hafen entgegenzunehmen.",
                        NotificationType.ERROR);
                    player.EmitGui("delivery:stopmydelivery");
                    player.ClearWaypoint();
                }

                _deliveryModule.UpdateGroupOpenDeliveriesUi(d.OrderGroupModel);
            });

        var pausedDeliveries = await _deliveryService.Where(d => d.Status == DeliveryState.PAUSED);
        pausedDeliveries.FindAll(d => (DateTime.Now - d.LastUsage).TotalMinutes >= _deliveryOptions.ResetTime)
            .ForEach(d => { d = ClearProductDelivery(d); });

        await _deliveryService.UpdateRange(acceptedDeliveries);
        await _deliveryService.UpdateRange(pausedDeliveries);

        await Task.CompletedTask;
    }

    private DeliveryModel ClearProductDelivery(DeliveryModel deliveryModel)
    {
        deliveryModel.Status = DeliveryState.OPEN;
        deliveryModel.SupplierCharacterId = null;
        deliveryModel.SupplierPhoneNumber = null;

        if (deliveryModel.PlayerVehicleModelId.HasValue)
        {
            var vehicle = Alt.GetAllVehicles().FindByDbId(deliveryModel.PlayerVehicleModelId.Value);
            if (vehicle is { Exists: true })
            {
                vehicle.DeleteData("AMOUNT_OF_PRODUCTS");

                deliveryModel.PlayerVehicleModelId = null;
            }
        }

        deliveryModel.PlayerVehicleModelId = null;

        return deliveryModel;
    }
}