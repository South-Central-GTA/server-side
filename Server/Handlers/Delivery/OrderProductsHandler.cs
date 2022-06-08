using System.Linq;
using AltV.Net.Async;
using Microsoft.Extensions.Options;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Configuration;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Delivery;
using Server.Database.Models.Group;
using Server.Modules.Delivery;
using Server.Modules.Group;
using Server.Modules.Phone;

namespace Server.Handlers.Delivery
{
    public class OrderProductsHandler
        : ISingletonScript
    {
        private readonly BankAccountService _bankAccountService;
        private readonly CompanyOptions _companyOptions;
        private readonly DeliveryModule _deliveryModule;
        private readonly DeliveryOptions _deliveryOptions;


        private readonly DeliveryService _deliveryService;

        private readonly GroupModule _groupModule;
        private readonly GroupService _groupService;
        private readonly PhoneModule _phoneModule;

        public OrderProductsHandler(
            IOptions<CompanyOptions> companyOptions,
            IOptions<DeliveryOptions> deliveryOptions,
            DeliveryService deliveryService,
            BankAccountService bankAccountService,
            GroupService groupService,
            GroupModule groupModule,
            PhoneModule phoneModule,
            DeliveryModule deliveryModule)
        {
            _companyOptions = companyOptions.Value;
            _deliveryOptions = deliveryOptions.Value;

            _deliveryService = deliveryService;
            _bankAccountService = bankAccountService;
            _groupService = groupService;

            _groupModule = groupModule;
            _phoneModule = phoneModule;
            _deliveryModule = deliveryModule;

            AltAsync.OnClient<ServerPlayer, int, int>("delivery:orderproducts", OnOrderProducts);
        }

        private async void OnOrderProducts(ServerPlayer player, int phoneId, int amount)
        {
            if (!player.Exists)
            {
                return;
            }

            if (amount <= 0)
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

            if (companyGroup.Products >= _companyOptions.MaxProducts)
            {
                await _phoneModule.SendNotification(phoneId,
                                                    PhoneNotificationType.DELIVERY,
                                                    "Leider sind, laut unseren Daten, Ihre Lager schon voll. Ihre Bestellung wurde annulliert.");
                return;
            }

            if (amount > 330)
            {
                await _phoneModule.SendNotification(phoneId,
                                                    PhoneNotificationType.DELIVERY,
                                                    "Leider können Sie nicht mehr als 330 Produkte pro Bestellung eintragen, Ihre Bestellung wurde annulliert.");
                return;
            }

            var diff = companyGroup.Products + amount;
            if (diff >= _companyOptions.MaxProducts)
            {
                await _phoneModule.SendNotification(phoneId,
                                                    PhoneNotificationType.DELIVERY,
                                                    "Leider konnte die letzte Bestellung nicht bearbeitet werden, sie übersteigt die maximale Kapazität Ihres Lagers.");
                return;
            }

            if (await _deliveryService.Exists(d => d.OrderGroupModelId == group.Id
                                                   && d.Status != DeliveryState.DELIVERD
                                                   && d.DeliveryType == DeliveryType.PRODUCT))
            {
                await _phoneModule.SendNotification(phoneId,
                                                    PhoneNotificationType.DELIVERY,
                                                    "Es besteht schon ein Lieferauftrag von Ihrem Unternehmen, bitte stornieren Sie diesen falls er noch nicht angenommen wurde um einen neuen zu erstellen.");
                return;
            }

            var price = amount * _deliveryOptions.ProductPrice;
            var bankAccount = await _bankAccountService.GetByOwningGroup(group.Id);
            if (bankAccount == null || bankAccount.Amount < price)
            {
                await _phoneModule.SendNotification(phoneId,
                                                    PhoneNotificationType.DELIVERY,
                                                    $"Leider kann Ihr Unternehmenskonto den Produktpreis von ${price} nicht decken. Die Abrechnung wird erst getätigt wenn die Bestellung abgeliefert wird.");
                return;
            }

            await _deliveryService.Add(new ProductDeliveryModel(companyGroup.Id, amount));
            await _phoneModule.SendNotification(phoneId,
                                                PhoneNotificationType.DELIVERY,
                                                "Wir haben die Bestellung erfolgreich in unser System aufgenommen.");

            await _deliveryModule.UpdateOpenDeliveriesUi();
        }
    }
}