using AltV.Net.Async;
using AltV.Net.Data;
using Microsoft.Extensions.Options;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Configuration;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.Delivery;

namespace Server.Handlers.Delivery;

public class RequestDeliveryMarkerHandler : ISingletonScript
{
    private readonly DeliveryService _deliveryService;

    private readonly HouseService _houseService;
    private readonly WorldLocationOptions _worldLocationOptions;

    public RequestDeliveryMarkerHandler(IOptions<WorldLocationOptions> worldLocationOptions,
        DeliveryService deliveryService, HouseService houseService, DeliveryModule deliveryModule)
    {
        _worldLocationOptions = worldLocationOptions.Value;

        _deliveryService = deliveryService;
        _houseService = houseService;

        AltAsync.OnClient<ServerPlayer>("delivery:requestmarker", OnRequestMarker);
    }

    private async void OnRequestMarker(ServerPlayer player)
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

        var house = await _houseService.Find(h => h.GroupModelId == delivery.OrderGroupModelId);
        if (house == null)
        {
            return;
        }

        switch (delivery.Status)
        {
            case DeliveryState.DELIVERD_BACK_TO_HARBOR:
            case DeliveryState.ACCEPTED:
                player.SetWaypoint(
                    new Position(_worldLocationOptions.HarbourSelectionPositionX,
                        _worldLocationOptions.HarbourSelectionPositionY,
                        _worldLocationOptions.HarbourSelectionPositionZ), 5, 1);
                break;
            case DeliveryState.LOADED:
                player.SetWaypoint(new Position(house.PositionX, house.PositionY, house.PositionZ), 5, 1);
                break;
            default:
                player.ClearWaypoint();
                break;
        }

        player.SendNotification("Marker wurde aktualisiert.", NotificationType.INFO);
    }
}