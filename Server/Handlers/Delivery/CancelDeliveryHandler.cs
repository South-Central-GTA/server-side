using System.Linq;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Group;
using Server.Modules.Delivery;
using Server.Modules.Group;

namespace Server.Handlers.Delivery
{
    public class CancelDeliveryHandler
        : ISingletonScript
    {
        private readonly DeliveryModule _deliveryModule;

        private readonly DeliveryService _deliveryService;

        private readonly GroupModule _groupModule;
        private readonly GroupService _groupService;

        public CancelDeliveryHandler(
            DeliveryService deliveryService,
            GroupService groupService,
            GroupModule groupModule,
            DeliveryModule deliveryModule)
        {
            _deliveryService = deliveryService;
            _groupService = groupService;

            _groupModule = groupModule;
            _deliveryModule = deliveryModule;

            AltAsync.OnClient<ServerPlayer, int>("delivery:canceldelivery", OnCancelDelivery);
        }

        private async void OnCancelDelivery(ServerPlayer player, int deliveryId)
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

            if (!await _groupModule.HasPermission(player.CharacterModel.Id, group.Id, GroupPermission.ORDER_PRODUCTS))
            {
                return;
            }

            var delivery = await _deliveryService.GetByKey(deliveryId);
            if (delivery == null || delivery.OrderGroupModelId != companyGroup.Id)
            {
                return;
            }

            await _deliveryService.Remove(delivery);

            await _deliveryModule.UpdateGroupDeliveriesUi(player, companyGroup.Id);
            await _deliveryModule.UpdateOpenDeliveriesUi();
        }
    }
}