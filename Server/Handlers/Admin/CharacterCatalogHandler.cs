using System.Collections.Generic;
using AltV.Net;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Banking;
using Server.Database.Models.Character;
using Server.Database.Models.Group;
using Server.Database.Models.Housing;

namespace Server.Handlers.Admin;

public class CharacterCatalogHandler : ISingletonScript
{
    private readonly BankAccountService _bankAccountService;
    private readonly CharacterService _characterService;
    private readonly GroupService _groupService;
    private readonly HouseService _houseService;
    private readonly VehicleCatalogService _vehicleCatalogService;
    private readonly VehicleService _vehicleService;

    public CharacterCatalogHandler(BankAccountService bankAccountService, CharacterService characterService,
        GroupService groupService, HouseService houseService, VehicleCatalogService vehicleCatalogService,
        VehicleService vehicleService)
    {
        _bankAccountService = bankAccountService;
        _characterService = characterService;
        _groupService = groupService;
        _houseService = houseService;
        _vehicleCatalogService = vehicleCatalogService;
        _vehicleService = vehicleService;

        AltAsync.OnClient<ServerPlayer>("charactercatalog:open", OnOpenCharacterCatalog);
        AltAsync.OnClient<ServerPlayer, int>("charactercatalog:requestdetails", OnRequestDetails);
    }

    private async void OnOpenCharacterCatalog(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.AccountModel.Permission.HasFlag(Permission.STAFF))
        {
            return;
        }

        player.EmitGui("charactercatalog:open", await _characterService.GetAll());
    }

    private async void OnRequestDetails(ServerPlayer player, int characterId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.AccountModel.Permission.HasFlag(Permission.STAFF))
        {
            return;
        }

        var character = await _characterService.GetByKey(characterId);
        if (character == null)
        {
            return;
        }

        var vehicles = await _vehicleService.Where(v => v.CharacterModelId == characterId);
        var vehicleDatas = new List<VehicleData>();

        foreach (var vehicle in vehicles)
        {
            var catalogVehicle = await _vehicleCatalogService.GetByKey(vehicle.Model.ToLower());
            if (catalogVehicle == null)
            {
                continue;
            }

            vehicleDatas.Add(new VehicleData
            {
                Id = vehicle.Id,
                DisplayName = catalogVehicle.DisplayName,
                DisplayClass = catalogVehicle.DisplayClass
            });
        }

        var houses = await _houseService.Where(h => h.CharacterModelId == characterId);
        var groups = await _groupService.GetGroupsByCharacter(characterId);
        var bankAccounts = await _bankAccountService.GetByCharacter(characterId);

        player.EmitGui("charactercatalog:requestdetails",
            new Data
            {
                Character = character,
                VehicleDatas = vehicleDatas,
                Houses = houses,
                Groups = groups,
                BankAccounts = bankAccounts
            });
    }

    private struct Data : IWritable
    {
        public CharacterModel Character { get; set; }
        public List<VehicleData> VehicleDatas { get; set; }
        public List<HouseModel> Houses { get; set; }
        public List<GroupModel> Groups { get; set; }
        public List<BankAccountModel> BankAccounts { get; set; }

        public void OnWrite(IMValueWriter writer)
        {
            writer.Name("character");

            CharacterModel.Serialize(Character, writer);

            writer.Name("vehicles");
            writer.BeginArray();

            foreach (var vehicleData in VehicleDatas)
            {
                VehicleData.Serialize(vehicleData, writer);
            }

            writer.EndArray();

            writer.Name("houses");
            writer.BeginArray();

            foreach (var house in Houses)
            {
                HouseModel.Serialize(house, writer);
            }

            writer.EndArray();

            writer.Name("groups");
            writer.BeginArray();

            foreach (var group in Groups)
            {
                GroupModel.Serialize(group, writer);
            }

            writer.EndArray();

            writer.Name("bankAccounts");
            writer.BeginArray();

            foreach (var bankAccount in BankAccounts)
            {
                BankAccountModel.Serialize(bankAccount, writer);
            }

            writer.EndArray();
        }
    }
}