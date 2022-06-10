using AltV.Net.Async;
using Microsoft.Extensions.Options;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Configuration;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Modules.Bank;
using Server.Modules.Group;
using Server.Modules.Houses;

namespace Server.Handlers.Company;

public class RequestCompanyHandler : ISingletonScript
{
    private readonly BankModule _bankModule;
    private readonly CompanyOptions _companyOptions;
    private readonly GroupModule _groupModule;
    private readonly HouseModule _houseModule;

    public RequestCompanyHandler(IOptions<CompanyOptions> companyOptions, GroupModule groupModule,
        BankModule bankModule, HouseModule houseModule)
    {
        _companyOptions = companyOptions.Value;

        _groupModule = groupModule;
        _bankModule = bankModule;
        _houseModule = houseModule;

        AltAsync.OnClient<ServerPlayer>("company:requestapp", OnRequestApp);
    }

    private async void OnRequestApp(ServerPlayer player)
    {
        await _groupModule.UpdateUi(player);
        await _bankModule.UpdateUi(player);
        await _houseModule.UpdateUi(player);

        player.EmitGui("company:setlicensetable", _companyOptions.Licenses);
    }
}