using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Modules.Bank;
using Server.Modules.Houses;

namespace Server.Handlers.House;

public class RentHouseHandler : ISingletonScript
{
    private readonly BankAccountService _bankAccountService;
    private readonly BankModule _bankModule;

    private readonly HouseModule _houseModule;

    private readonly HouseService _houseService;

    public RentHouseHandler(HouseService houseService, BankAccountService bankAccountService, HouseModule houseModule,
        BankModule bankModule)
    {
        _houseService = houseService;
        _bankAccountService = bankAccountService;

        _houseModule = houseModule;
        _bankModule = bankModule;

        AltAsync.OnClient<ServerPlayer, int>("house:rent", OnRentHouse);
    }

    private async void OnRentHouse(ServerPlayer player, int bankAccountId)
    {
        if (!player.Exists)
        {
            return;
        }

        var house = await _houseService.GetByDistance(player.Position);
        if (house == null)
        {
            player.SendNotification("Es ist keine mietbare Immobilie in der Nähe.", NotificationType.ERROR);
            return;
        }

        var bankAccount = await _bankAccountService.GetByKey(bankAccountId);
        if (bankAccount == null)
        {
            player.SendNotification("Das Bankkonto existiert nicht mehr.", NotificationType.ERROR);
            return;
        }

        if (!await _bankModule.Withdraw(bankAccount, house.Price, false, "Immobilie angemietet"))
        {
            player.SendNotification("Nicht genug Geld auf dem Bankkonto.", NotificationType.ERROR);
            return;
        }

        if (house.HasOwner)
        {
            player.SendNotification("Diese Immobilie hat schon einen Besitzer.", NotificationType.ERROR);
            return;
        }

        await _houseModule.ResetOwner(house);
        await _houseModule.SetOwner(player.CharacterModel, house);

        house.RentBankAccountId = bankAccountId;
        await _houseService.Update(house);

        await _houseModule.UpdateOnClient(house);

        player.SendNotification("Erfolgreich Immobilie angemietet.", NotificationType.SUCCESS);
    }
}