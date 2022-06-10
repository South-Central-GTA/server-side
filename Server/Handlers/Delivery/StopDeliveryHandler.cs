using AltV.Net;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Delivery;
using Server.Modules.Delivery;

namespace Server.Handlers.Delivery;

public class StopDeliveryHandler : ISingletonScript
{
    private readonly DeliveryModule _deliveryModule;

    private readonly DeliveryService _deliveryService;

    public StopDeliveryHandler(DeliveryService deliveryService, DeliveryModule deliveryModule)
    {
        _deliveryService = deliveryService;
        _deliveryModule = deliveryModule;

        AltAsync.OnClient<ServerPlayer>("delivery:stopdelivery", OnStopDelivery);
    }

    private async void OnStopDelivery(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        var delivery = await _deliveryService.Find(d => d.SupplierCharacterId == player.CharacterModel.Id);
        if (delivery == null)
        {
            return;
        }

        if (delivery.PlayerVehicleModelId.HasValue)
        {
            var vehicle = Alt.GetAllVehicles().FindByDbId(delivery.PlayerVehicleModelId.Value);
            if (vehicle is { Exists: true })
            {
                if (delivery is ProductDeliveryModel productDelivery)
                {
                    var trailerOrMainVeh = player.Vehicle.Attached ?? player.Vehicle;

                    trailerOrMainVeh.GetData("AMOUNT_OF_PRODUCTS", out int amountOfProducts);
                    productDelivery.ProductsRemaining = amountOfProducts;
                }
            }
        }

        delivery.Status = DeliveryState.OPEN;
        delivery.SupplierCharacterId = null;
        delivery.PlayerVehicleModelId = null;

        await _deliveryService.Update(delivery);

        await _deliveryModule.UpdatePlayerOpenDeliveriesUi(player);
        await _deliveryModule.UpdateOpenDeliveriesUi();

        player.EmitGui("delivery:stopmydelivery");
        player.ClearWaypoint();

        player.SendNotification("Der Auftrag wurde abgebrochen.", NotificationType.INFO);
    }
}