using System.Linq;
using AltV.Net;
using AltV.Net.Async;
using Microsoft.Extensions.Options;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Configuration;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Group;
using Server.Modules.Bank;
using Server.Modules.Group;

namespace Server.Handlers.Company;

public class SellLicenseHandler : ISingletonScript
{
    private readonly CompanyOptions _companyOptions;
    private readonly BankAccountService _bankAccountService;
    private readonly GroupService _groupService;
    private readonly BankModule _bankModule;
    private readonly GroupModule _groupModule;

    public SellLicenseHandler(
        IOptions<CompanyOptions> companyOptions,
        GroupService groupService,
        BankAccountService bankAccountService,
        GroupModule groupModule,
        BankModule bankModule)
    {
        _companyOptions = companyOptions.Value;

        _groupService = groupService;
        _bankAccountService = bankAccountService;

        _groupModule = groupModule;
        _bankModule = bankModule;

        AltAsync.OnClient<ServerPlayer, int, int, LicensesFlags>("company:selllicenses", OnSellLicenses);
    }

    private async void OnSellLicenses(ServerPlayer player, int phoneId, int companyId, LicensesFlags licensesFlags)
    {
        var groups = await _groupService.GetGroupsByCharacter(player.CharacterModel.Id);
        var group = groups?.FirstOrDefault(g => g.GroupType == GroupType.COMPANY && g.Id == companyId);
        if (group == null)
        {
            return;
        }

        var companyGroup = (CompanyGroupModel)group;

        if (!await _groupModule.HasPermission(player.CharacterModel.Id, group.Id, GroupPermission.SELL_LICENSES))
        {
            return;
        }

        var license = _companyOptions.Licenses.Find(l => l.License == licensesFlags);
        if (license == null)
        {
            return;
        }

        var bankAccount = await _bankAccountService.Find(ba =>
                                                             ba.Type == OwnableAccountType.GROUP
                                                             && ba.GroupRankAccess.Any(gra => gra.GroupModelId == companyGroup.Id));

        var price = (int)(license.Price * 0.5f);

        await _bankModule.Deposit(bankAccount, price, $"Lizenz '{license.Name}' gekauft");

        foreach (var target in
                 group.Members.Select(m => Alt.GetAllPlayers().FindPlayerByCharacterId(m.CharacterModelId)))
        {
            await _bankModule.UpdateUi(target);
        }

        await _bankModule.UpdateUi(player);

        companyGroup.LicensesFlags &= ~license.License;
        companyGroup.PurchasedLicenses--;

        await _groupService.Update(companyGroup);
        await _groupModule.UpdateGroupUi(companyGroup);
    }
}