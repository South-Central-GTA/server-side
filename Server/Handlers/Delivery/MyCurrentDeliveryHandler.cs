using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.DataAccessLayer.Services;
using Server.Database.Models.Delivery;

namespace Server.Handlers.Delivery;

public class MyCurrentDeliveryHandler : ISingletonScript
{
    private readonly DeliveryService _deliveryService;

    public MyCurrentDeliveryHandler(DeliveryService deliveryService)
    {
        _deliveryService = deliveryService;

        AltAsync.OnClient<ServerPlayer>("delivery:getmycurrentdelivery", OnGetMyCurrentDelivery);
    }

    private async void OnGetMyCurrentDelivery(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        var delivery = await _deliveryService.Find(d => d.SupplierCharacterId == player.CharacterModel.Id);

        if (delivery is ProductDeliveryModel productDelivery)
        {
            player.EmitGui("delivery:sendmycurrentdelivery", productDelivery);
        }
    }
}