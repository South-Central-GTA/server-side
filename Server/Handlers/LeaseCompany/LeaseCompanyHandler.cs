using System.Linq;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Housing;
using Server.Modules.Bank;
using Server.Modules.Group;
using Server.Modules.Houses;

namespace Server.Handlers.LeaseCompany;

public class LeaseCompanyHandler : ISingletonScript
{
    private readonly BankAccountService _bankAccountService;
    private readonly BankModule _bankModule;
    private readonly GroupModule _groupModule;
    private readonly GroupService _groupService;

    private readonly HouseModule _houseModule;

    private readonly HouseService _houseService;

    public LeaseCompanyHandler(HouseService houseService, BankAccountService bankAccountService,
        GroupService groupService, HouseModule houseModule, BankModule bankModule, GroupModule groupModule)
    {
        _houseService = houseService;
        _bankAccountService = bankAccountService;
        _groupService = groupService;

        _houseModule = houseModule;
        _bankModule = bankModule;
        _groupModule = groupModule;

        AltAsync.OnClient<ServerPlayer, int>("company:lease", OnLeaseCompany);
    }

    private async void OnLeaseCompany(ServerPlayer player, int bankAccountId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (await _houseService.GetByDistance(player.Position) is not LeaseCompanyHouseModel leaseCompanyHouse)
        {
            player.SendNotification("Es ist kein pachtbarer Unternehmenssitz in der Nähe deines Charakters.",
                NotificationType.ERROR);
            return;
        }

        var groups = await _groupService.GetGroupsByCharacter(player.CharacterModel.Id);
        var group = groups?.FirstOrDefault(g => g.GroupType == GroupType.COMPANY);
        if (group == null)
        {
            player.SendNotification("Dein Charakter ist in keinem Unternehmen.", NotificationType.ERROR);
            return;
        }

        if (!_groupModule.IsOwner(player, group))
        {
            player.SendNotification("Dein Charakter ist nicht der Eigentümer des Unternehmens.",
                NotificationType.ERROR);
            return;
        }

        var bankAccount = await _bankAccountService.GetByKey(bankAccountId);
        if (bankAccount == null)
        {
            player.SendNotification("Das Bankkonto existiert nicht mehr.", NotificationType.ERROR);
            return;
        }

        if (!await _bankModule.Withdraw(bankAccount, leaseCompanyHouse.Price, false, "Unternehmenssitz gepachtet"))
        {
            player.SendNotification("Nicht genug Geld auf dem Bankkonto.", NotificationType.ERROR);
            return;
        }

        await _houseModule.ResetOwner(leaseCompanyHouse);

        leaseCompanyHouse.GroupModelId = group.Id;
        leaseCompanyHouse.CharacterModelId = player.CharacterModel.Id;
        leaseCompanyHouse.RentBankAccountId = bankAccountId;

        await _houseService.Update(leaseCompanyHouse);
        await _houseModule.UpdateOnClient(leaseCompanyHouse);

        player.SendNotification(
            "Erfolgreich Unternehmenssitz gepachtet erstelle dir mit /creategkey einen Gruppenschlüssel.",
            NotificationType.SUCCESS);
    }
}