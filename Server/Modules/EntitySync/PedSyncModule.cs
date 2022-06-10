using System.Collections.Generic;
using System.Linq;
using AltV.Net.Data;
using AltV.Net.EntitySync;
using Microsoft.Extensions.Options;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Configuration;
using Server.Core.Entities;
using Server.Data.Enums.EntitySync;
using Server.Database.Models.Character;
using Server.Database.Models.Housing;

namespace Server.Modules.EntitySync;

public class PedSyncModule : ISingletonScript
{
    private readonly Dictionary<int, ServerPed> _cashierPeds = new();

    private readonly CompanyOptions _companyOptions;
    private readonly Dictionary<ulong, ServerPed> _peds = new();
    private readonly Dictionary<int, ServerPed> _playerPeds = new();

    public PedSyncModule(IOptions<CompanyOptions> companyOptions)
    {
        _companyOptions = companyOptions.Value;
    }

    public void CreateTemp(ServerPlayer player, string model, Position position, float heading, int dimension,
        ServerVehicle vehicle, int seat = 0, uint streamRange = 200)
    {
        var serverPed = Create(model, position, heading, dimension, streamRange, vehicle, seat);
        _playerPeds.Add(player.Id, serverPed);
    }

    public ServerPed Create(string model, Position position, float heading, int dimension, uint streamRange = 200,
        ServerVehicle? vehicle = null, int seat = 0, CharacterModel? characterModel = null)
    {
        var serverPed = new ServerPed(position, dimension, streamRange)
        {
            Model = model,
            Heading = heading,
            Vehicle = vehicle,
            Seat = seat,
            CharacterModel = characterModel
        };

        AltEntitySync.AddEntity(serverPed);
        _peds.Add(serverPed.Id, serverPed);

        return serverPed;
    }

    public bool Delete(ulong objectId)
    {
        var serverPed = Get(objectId);
        if (serverPed == null)
        {
            return false;
        }

        AltEntitySync.RemoveEntity(serverPed);
        _peds.Remove(serverPed.Id);

        return true;
    }

    public void Delete(ServerPlayer player)
    {
        if (!_peds.TryGetValue(player.Id, out var serverPed))
        {
            return;
        }

        AltEntitySync.RemoveEntity(serverPed);
        _playerPeds.Remove(player.Id);
        _peds.Remove(serverPed.Id);
    }


    public void DeleteAll()
    {
        foreach (var serverBlip in GetAll())
        {
            AltEntitySync.RemoveEntity(serverBlip);
        }

        _peds.Clear();
        _playerPeds.Clear();
        _cashierPeds.Clear();
    }

    public ServerPed? GetPlayer(ServerPlayer player)
    {
        return !_peds.TryGetValue(player.Id, out var serverPed) ? null : serverPed;
    }

    public ServerPed? Get(ulong objectId)
    {
        if (!AltEntitySync.TryGetEntity(objectId, (ulong)EntityType.PED, out var entity))
        {
            return null;
        }

        return entity is not ServerPed serverPed ? default : serverPed;
    }

    public List<ServerPed> GetAll()
    {
        return _peds.Select(entity => entity.Value).ToList();
    }

    public void CreateCashier(LeaseCompanyHouseModel leaseCompanyHouseModel)
    {
        if (leaseCompanyHouseModel.PlayerDuty || !leaseCompanyHouseModel.HasCashier ||
            !leaseCompanyHouseModel.CashierX.HasValue || !leaseCompanyHouseModel.CashierY.HasValue ||
            !leaseCompanyHouseModel.CashierZ.HasValue || !leaseCompanyHouseModel.CashierHeading.HasValue)
        {
            return;
        }

        var serverPed = Create(_companyOptions.Types[leaseCompanyHouseModel.LeaseCompanyType].Cashier,
            new Position(leaseCompanyHouseModel.CashierX.Value, leaseCompanyHouseModel.CashierY.Value,
                leaseCompanyHouseModel.CashierZ.Value), leaseCompanyHouseModel.CashierHeading.Value, 0);

        _cashierPeds.Add(leaseCompanyHouseModel.Id, serverPed);
    }

    public void UpdateCashier(LeaseCompanyHouseModel leaseCompanyHouseModel, Position position, float heading)
    {
        if (!_cashierPeds.TryGetValue(leaseCompanyHouseModel.Id, out var serverPed))
        {
            CreateCashier(leaseCompanyHouseModel);
            return;
        }

        serverPed.Position = position;
        serverPed.Heading = heading;
    }

    public void RemoveCashier(int leaseCompanyId)
    {
        if (!_cashierPeds.TryGetValue(leaseCompanyId, out var serverPed))
        {
            return;
        }

        AltEntitySync.RemoveEntity(serverPed);
        _cashierPeds.Remove(leaseCompanyId);
    }
}