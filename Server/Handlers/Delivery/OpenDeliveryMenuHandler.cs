using System.Linq;
using AltV.Net.Async;
using Microsoft.Extensions.Options;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Configuration;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Group;
using Server.Modules.Group;

namespace Server.Handlers.Delivery
{
    public class OpenDeliveryMenuHandler
        : ISingletonScript
    {
        private readonly CompanyOptions _companyOptions;
        private readonly DeliveryService _deliveryService;

        private readonly GroupModule _groupModule;
        private readonly GroupService _groupService;

        public OpenDeliveryMenuHandler(
            IOptions<CompanyOptions> companyOptions,
            DeliveryService deliveryService,
            GroupService groupService,
            GroupModule groupModule)
        {
            _companyOptions = companyOptions.Value;

            _deliveryService = deliveryService;
            _groupService = groupService;

            _groupModule = groupModule;

            AltAsync.OnClient<ServerPlayer>("delivery:requestmenu", OnRequestMenu);
        }

        private async void OnRequestMenu(ServerPlayer player)
        {
            if (!player.Exists)
            {
                return;
            }

            var canUseThisApp = false;
            var canSeeOpenDeliveries = false;
            var hasOpenDelivery = false;
            var products = 0;

            var groups = await _groupService.GetGroupsByCharacter(player.CharacterModel.Id);
            var group = groups?.FirstOrDefault(g => g.GroupType == GroupType.COMPANY);
            if (group != null)
            {
                var companyGroup = (CompanyGroupModel)group;

                canUseThisApp = await _groupModule.HasPermission(player.CharacterModel.Id, group.Id, GroupPermission.ORDER_PRODUCTS);
                canSeeOpenDeliveries = companyGroup.LicensesFlags.HasFlag(LicensesFlags.GOODS_TRANSPORT);
                products = companyGroup.Products;
            }

            if (await _deliveryService.Exists(d => d.SupplierCharacterId == player.CharacterModel.Id))
            {
                hasOpenDelivery = true;
            }

            player.EmitGui("delivery:setup", canUseThisApp, canSeeOpenDeliveries, hasOpenDelivery, products, _companyOptions.MaxProducts);
        }
    }
}