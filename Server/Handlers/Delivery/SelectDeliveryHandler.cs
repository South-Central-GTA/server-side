using AltV.Net.Async;
using AltV.Net.Data;
using Microsoft.Extensions.Options;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Configuration;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.Delivery;
using Server.Modules.Phone;

namespace Server.Handlers.Delivery;

public class SelectDeliveryHandler : ISingletonScript
{
    private readonly DeliveryModule _deliveryModule;
    private readonly DeliveryOptions _deliveryOptions;

    private readonly DeliveryService _deliveryService;

    private readonly PhoneModule _phoneModule;
    private readonly WorldLocationOptions _worldLocationOptions;

    public SelectDeliveryHandler(IOptions<DeliveryOptions> deliveryOptions,
        IOptions<WorldLocationOptions> worldLocationOptions, DeliveryService deliveryService, PhoneModule phoneModule,
        DeliveryModule deliveryModule)
    {
        _deliveryOptions = deliveryOptions.Value;
        _worldLocationOptions = worldLocationOptions.Value;

        _deliveryService = deliveryService;

        _phoneModule = phoneModule;
        _deliveryModule = deliveryModule;

        AltAsync.OnClient<ServerPlayer, int, int>("delivery:selectdelivery", OnSelectDelivery);
    }

    private async void OnSelectDelivery(ServerPlayer player, int phoneId, int deliveryId)
    {
        if (!player.Exists)
        {
            return;
        }

        var delivery = await _deliveryService.GetByKey(deliveryId);
        if (delivery == null)
        {
            await _phoneModule.SendNotification(phoneId, PhoneNotificationType.DELIVERY,
                "Die Bestellung konnte nicht mehr in unserem System gefunden werden.");
            return;
        }

        if (delivery.Status != DeliveryState.OPEN)
        {
            await _phoneModule.SendNotification(phoneId, PhoneNotificationType.DELIVERY,
                "Leider ist der Auftrag gerade von einem anderen Fahrer angenommen worden. Tut uns leid, die Aufträge vergeben wir in Echtzeit!");
            return;
        }

        var phone = await _phoneModule.GetById(phoneId);
        if (phone == null)
        {
            return;
        }

        delivery.Status = DeliveryState.ACCEPTED;
        delivery.SupplierCharacterId = player.CharacterModel.Id;
        delivery.SupplierFullName = player.CharacterModel.Name;
        delivery.SupplierPhoneNumber = phone.PhoneNumber;

        await _deliveryService.Update(delivery);
        await _deliveryModule.UpdateOpenDeliveriesUi();

        player.EmitGui("delivery:openmydelivery", delivery);

        player.SetWaypoint(
            new Position(_worldLocationOptions.HarbourSelectionPositionX,
                _worldLocationOptions.HarbourSelectionPositionY, _worldLocationOptions.HarbourSelectionPositionZ), 5,
            1);

        await _phoneModule.SendNotification(phoneId, PhoneNotificationType.DELIVERY,
            $"Der Auftrag wurde auf Ihren Namen gutgeschrieben, Sie haben nun {_deliveryOptions.PickupTime} Minuten Zeit diesen am Hafen aufzuladen.");
    }
}