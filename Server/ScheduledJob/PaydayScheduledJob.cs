using System;
using System.Linq;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Elements.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Server.Core.Callbacks;
using Server.Core.Configuration;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Housing;
using Server.Modules;
using Server.Modules.Bank;
using Server.Modules.Group;
using Server.Modules.Houses;

namespace Server.ScheduledJob;

public class PaydayScheduledJob
    : ScheduledJob
{
    private readonly BankAccountService _bankAccountService;

    private readonly ILogger<PaydayScheduledJob> _logger;
    private readonly WorldLocationOptions _worldLocationOptions;
    private readonly CompanyOptions _companyOptions;
    private readonly GameOptions _gameOptions;
    private readonly GroupService _groupService;
    private readonly HouseService _houseService;
    private readonly PublicGarageEntryService _publicGarageEntryService;
    private readonly VehicleCatalogService _vehicleCatalogService;
    private readonly VehicleService _vehicleService;
    private readonly BankModule _bankModule;
    private readonly GroupModule _groupModule;
    private readonly HouseModule _houseModule;

    private int _lastMinute = 1;

    public PaydayScheduledJob(
        ILogger<PaydayScheduledJob> logger,
        IOptions<GameOptions> gameOptions,
        IOptions<CompanyOptions> companyOptions,
        IOptions<WorldLocationOptions> worldLocationOptions,
        BankModule bankModule,
        GroupModule groupModule,
        BankAccountService bankAccountService,
        PublicGarageEntryService publicGarageEntryService,
        VehicleService vehicleService,
        VehicleCatalogService vehicleCatalogService,
        GroupService groupService,
        HouseService houseService,
        HouseModule houseModule)
        : base(TimeSpan.FromMinutes(1))
    {
        _logger = logger;
        _gameOptions = gameOptions.Value;
        _companyOptions = companyOptions.Value;
        _worldLocationOptions = worldLocationOptions.Value;
        
        _bankAccountService = bankAccountService;
        _publicGarageEntryService = publicGarageEntryService;
        _vehicleService = vehicleService;
        _vehicleCatalogService = vehicleCatalogService;
        _groupService = groupService;
        _houseService = houseService;
        
        _bankModule = bankModule;
        _groupModule = groupModule;
        _houseModule = houseModule;
    }

    public override async Task Action()
    {
        var minute = DateTime.Now.Minute;
        if (minute != _lastMinute && minute % _gameOptions.PayDayMinute == 0)
        {
            _lastMinute = minute;

            var callback = new AsyncFunctionCallback<IPlayer>(async player =>
            {
                if (!player.Exists)
                {
                    return;
                }

                var serverPlayer = (ServerPlayer)player;

                if (!serverPlayer.IsSpawned)
                {
                    return;
                }

                if (!await _bankModule.HasBankAccount(serverPlayer))
                {
                    return;
                }

                await HandleGroupIncome(serverPlayer);
                await HandleJobIncome(serverPlayer);
                await HandleRents(serverPlayer);
                await HandlePublicGarage(serverPlayer);
                await HandleTaxes(serverPlayer);

                await _bankModule.UpdateUi(serverPlayer);

                serverPlayer.SendNotification("(Debug): Es ist Zahltag!", NotificationType.WARNING);

                await Task.CompletedTask;
            });

            await Alt.ForEachPlayers(callback);

            var houses = await _houseService.GetAll();
            foreach (var leaseCompanyHouse in houses.Where(h => h.HouseType == HouseType.COMPANY
                                                                && h.GroupModelId.HasValue).Cast<LeaseCompanyHouseModel>())
            {
                var revenue = _companyOptions.RevenueEachPayday[leaseCompanyHouse.LeaseCompanyType];

                if (leaseCompanyHouse.HasCashier)
                {
                    revenue -= _companyOptions.CashierPayDayCosts;
                }

                var ownerGroup = await _groupService.GetByKey(leaseCompanyHouse.GroupModelId);
                if (ownerGroup == null)
                {
                    continue;
                }

                var bankAccount = await _bankAccountService.Find(ba =>
                                                                     ba.Type == OwnableAccountType.GROUP
                                                                     && ba.GroupRankAccess.Any(gra => gra.GroupModelId == ownerGroup.Id));
                if (bankAccount == null)
                {
                    continue;
                }

                switch (revenue)
                {
                    case 0:
                        continue;
                    case < 0:
                        await _bankModule.Withdraw(bankAccount, Math.Abs(revenue), true, "Pachtbarer Unternehmenssitz Umsatz");
                        break;
                    default:
                        await _bankModule.Deposit(bankAccount, revenue, "Pachtbarer Unternehmenssitz Umsatz");
                        break;
                }

                await _groupModule.UpdateGroupUi(ownerGroup);
            }
        }
    }

    private async Task HandleGroupIncome(ServerPlayer player)
    {
        var groups = await _groupService.GetGroupsByCharacter(player.CharacterModel.Id);
        if (groups == null)
        {
            return;
        }

        foreach (var group in groups)
        {
            var member = group.Members.FirstOrDefault(m => m.CharacterModelId == player.CharacterModel.Id);
            if (member != null)
            {
                if (member.Salary == 0)
                {
                    continue;
                }

                var bankAccount = await _bankAccountService.GetByKey(member.BankAccountId);
                if (bankAccount != null)
                {
                    var groupBankAccount = await _bankAccountService.GetByOwningGroup(group.Id);
                    var success = await _bankModule.Withdraw(groupBankAccount, (int)member.Salary, false, $"Mitarbeiter '{member.CharacterModel.Name}' Gehalt");
                    if (success)
                    {
                        await _bankModule.Deposit(bankAccount, (int)member.Salary, "Gehalt");
                    }
                }
            }
        }
    }

    private async Task HandleJobIncome(ServerPlayer player)
    {
        if (player.CharacterModel.JobModel == null)
        {
            return;
        }

        var definedJobData = _gameOptions.DefinedJobs.Find(d => d.Id == player.CharacterModel.JobModel.JobId);
        if (definedJobData == null)
        {
            return;
        }

        var bankAccount = await _bankAccountService.GetByKey(player.CharacterModel.JobModel.BankAccountId);
        if (bankAccount == null)
        {
            return;
        }

        await _bankModule.Deposit(bankAccount, (int)definedJobData.Salary, "Gehalt");
    }

    private async Task HandleRents(ServerPlayer player)
    {
        var houses = await _houseService.Where(h => h.CharacterModelId == player.CharacterModel.Id && h.Rentable);
        if (houses.Count == 0)
        {
            return;
        }

        foreach (var house in houses)
        {
            if (!house.RentBankAccountId.HasValue)
            {
                continue;
            }
            
            var bankAccount = await _bankAccountService.GetByKey(house.RentBankAccountId);
            if (bankAccount == null)
            {
                continue;
            }
            
            if (!await _bankModule.HasPermission(player, bankAccount, BankingPermission.TRANSFER))
            {
                player.SendNotification($"Dein Charakter hat keine Transferrechte mehr für das Konto {bankAccount.BankDetails} der Mietvertrag mit der Immobilie ({house.Id}) wurde aufgelöst.", NotificationType.WARNING);
                await _houseModule.ResetOwner(house);
                continue;
            }

            if (!await _bankModule.Withdraw(bankAccount, house.Price, false, $"Miete für Immobilie #'{house.Id}'"))
            {
                player.SendNotification($"Das angegebene Konto {bankAccount.BankDetails} kann die Kosten für den Mietvertrag der Immobilie ({house.Id}) nicht decken daher wurde der Vertrag aufgelöst.", NotificationType.WARNING);
                await _houseModule.ResetOwner(house);
            }
        }
        
        await Task.CompletedTask;
    }
    
    private async Task HandlePublicGarage(ServerPlayer player)
    {
        var vehicles = await _vehicleService.Where(v => v.CharacterModelId == player.CharacterModel.Id
                                                        && v.VehicleState == VehicleState.IN_GARAGE);
        if (vehicles.Count == 0)
        {
            return;
        }

        foreach (var vehicle in vehicles)
        {
            var publicGarageEntry = await _publicGarageEntryService.Find(p => p.PlayerVehicleModelId == vehicle.Id);
            if (publicGarageEntry == null)
            {
                continue;
            }
            
            var publicGarageData = _worldLocationOptions.PublicGarages.Find(p => p.Id == publicGarageEntry.GarageId);
            if (publicGarageData == null)
            {
                continue;
            }

            var catalogVehicle = await _vehicleCatalogService.GetByKey(vehicle.Model.ToLower());
            if (catalogVehicle == null)
            {
                continue;
            }

            var bankAccount = await _bankAccountService.GetByKey(publicGarageEntry.BankAccountId);
            if (bankAccount == null)
            {
                continue;
            }

            if (!await _bankModule.HasPermission(player, bankAccount, BankingPermission.TRANSFER))
            {
                player.SendNotification($"Dein Charakter hat keine Transferrechte mehr für das Konto {bankAccount.BankDetails} das Fahrzeug {catalogVehicle.DisplayName} wird ausgeparkt.", NotificationType.WARNING);
                // TODO: Implement "unparking" of vehicle.
                continue;
            }

            var costs = (int)(catalogVehicle.Price * publicGarageData.CostsPercentageOfVehiclePrice);
            if (!await _bankModule.Withdraw(bankAccount, costs, false, $"Dauerparken für '{catalogVehicle.DisplayName}'"))
            {
                player.SendNotification($"Das angegebene Konto {bankAccount.BankDetails} kann die Parkkosten für das Fahrzeug {catalogVehicle.DisplayName} nicht mehr decken daher wurde es ausgeparkt.", NotificationType.WARNING);
            }
        }

        await Task.CompletedTask;
    }

    private async Task HandleTaxes(ServerPlayer player)
    {
        var bankAccounts = await _bankAccountService.GetByOwner(player.CharacterModel.Id);
        bankAccounts = bankAccounts.FindAll(b => b.Amount >= _gameOptions.BankMoneyUntilTaxes);

        foreach (var bankAccount in bankAccounts)
        {
            var taxes = (int)(bankAccount.Amount * _gameOptions.TaxesExchangeRate);
            await _bankModule.Withdraw(bankAccount, taxes, false, "Steuern");
        }
        
        await Task.CompletedTask;
    }
}