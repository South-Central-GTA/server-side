using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Banking;
using Server.Database.Models.File;
using Server.Database.Models.Housing;
using Server.Database.Models.Mdc;
using Server.Modules.FileSystem;

namespace Server.Modules.MDC;

public class PoliceMdcModule
    : ISingletonScript
{
    public CallSign CallSign { get; }

    private readonly EmergencyCallService _emergencyCallService;
    private readonly GroupFactionService _groupFactionService;
    private readonly CriminalRecordService _criminalRecordService;
    private readonly MdcNoteService _mdcNoteService;
    private readonly CharacterService _characterService;
    private readonly VehicleService _vehicleService;
    private readonly VehicleCatalogService _vehicleCatalogService;
    private readonly HouseService _houseService;
    private readonly BankAccountService _bankAccountService;
    private readonly ItemPhoneService _itemPhoneService;
    private readonly ItemWeaponService _itemWeaponService;
    private readonly MailAccountService _mailAccountService;
    private readonly RegistrationOfficeService _registrationOfficeService;
    
    public PoliceMdcModule(
        GroupFactionService groupFactionService, 
        EmergencyCallService emergencyCallService,
        CriminalRecordService criminalRecordService, 
        MdcNoteService mdcNoteService,
        CharacterService characterService, 
        VehicleService vehicleService, 
        VehicleCatalogService vehicleCatalogService,
        HouseService houseService, 
        BankAccountService bankAccountService, 
        ItemPhoneService itemPhoneService, 
        ItemWeaponService itemWeaponService, 
        MailAccountService mailAccountService, 
        RegistrationOfficeService registrationOfficeService)
    {
        CallSign = new CallSign(groupFactionService);
        _groupFactionService = groupFactionService;
        _emergencyCallService = emergencyCallService;
        _characterService = characterService;
        _vehicleService = vehicleService;
        _vehicleCatalogService = vehicleCatalogService;
        _houseService = houseService;
        _bankAccountService = bankAccountService;
        _itemPhoneService = itemPhoneService;
        _itemWeaponService = itemWeaponService;
        _mailAccountService = mailAccountService;
        _registrationOfficeService = registrationOfficeService;
        _criminalRecordService = criminalRecordService;
        _mdcNoteService = mdcNoteService;
    }

    public async Task<List<EmergencyCallModel>> GetEmergencyCalls()
    {
        var emergencyCalls = await _emergencyCallService.GetAll();
        return emergencyCalls.Where(e => e.FactionType == FactionType.POLICE_DEPARTMENT).ToList();
    }

    public async Task UpdateEmergencyCallsUi(ServerPlayer player)
    {
        var factionGroup = await _groupFactionService.GetFactionByCharacter(player.CharacterModel.Id);
        if (factionGroup == null)
        {
            return;
        }
        
        foreach (var target in factionGroup.Members
                                           .Select(groupMember => Alt.GetAllPlayers().FindPlayerByCharacterId(groupMember.CharacterModelId))
                                           .Where(serverPlayer => serverPlayer != null))
        {
            target.EmitGui("policemdc:updateemergencycalls", await GetEmergencyCalls()); 
        }
    }

    public async Task OpenCharacterRecord(ServerPlayer player, string targetCharacterId)
    {
        var character = await _characterService.GetByKey(int.Parse(targetCharacterId));
        if (character == null)
        {
            return;
        }

        var isRegistered = await _registrationOfficeService.IsRegistered(character.Id);

        var records = await _criminalRecordService.Where(r => r.CharacterModelId == character.Id);
        var notes = await _mdcNoteService.Where(r => r.TargetModelId == targetCharacterId && r.Type == MdcSearchType.NAME);

       
        var vehicles = await _vehicleService.Where(v => v.CharacterModelId == character.Id);
        var vehicleDatas = new List<VehicleData>();

        if (isRegistered)
        {
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
                    DisplayClass = catalogVehicle.DisplayClass,
                    NumberPlateText = vehicle.NumberplateText
                });
            }
        }


        var houses = isRegistered
            ? await _houseService.Where(h => h.CharacterModelId == character.Id)
            : new List<HouseModel>();
        
        var bankAccounts = isRegistered 
            ? await _bankAccountService.GetByCharacter(character.Id) 
            : new List<BankAccountModel>();
        
        var phoneModels = await _itemPhoneService.Where(p => p.InitialOwnerId == character.Id);
        var phoneNumbers = isRegistered 
            ? phoneModels.Select(p => p.PhoneNumber).ToList()
            : new List<string>();
        
        player.EmitLocked("policemdc:opencharacterrecord", character, records, notes, vehicleDatas, houses, bankAccounts, phoneNumbers);
    }

    public async Task OpenPhoneRecord(ServerPlayer player, string targetPhoneId)
    {        
        var nodes = await _mdcNoteService.Where(r => r.TargetModelId == targetPhoneId && r.Type == MdcSearchType.NUMBER);
        var phone = await _itemPhoneService.GetByKey(int.Parse(targetPhoneId));
        if (phone == null)
        {
            return;
        }

        var ownerCharacterName = string.Empty;
        var ownerCharacter = await _characterService.GetByKey(phone.InitialOwnerId);
        if (ownerCharacter != null)
        {
            ownerCharacterName = ownerCharacter.Name;
        }
        
        player.EmitGui("policemdc:openphonerecord", phone.Id, phone.PhoneNumber, ownerCharacterName, nodes);
    }

    public async Task OpenVehicleRecord(ServerPlayer player, string targetVehicleId)
    {
        var nodes = await _mdcNoteService.Where(r => r.TargetModelId == targetVehicleId && r.Type == MdcSearchType.VEHICLE);
        var vehicle = await _vehicleService.GetByKey(int.Parse(targetVehicleId));
        if (vehicle == null)
        {
            return;
        }
        
        var catalogVehicle = await _vehicleCatalogService.GetByKey(vehicle.Model.ToLower());
        if (catalogVehicle == null)
        {
            return;
        }
        
        var ownerName = string.Empty;
        if (vehicle.CharacterModelId.HasValue && vehicle.CharacterModel != null)
        {
            ownerName = vehicle.CharacterModel.Name;
        }
        else if (vehicle.GroupModelOwnerId.HasValue && vehicle.GroupModelOwner != null)
        {
            ownerName = vehicle.GroupModelOwner.Name;
        }
        
        player.EmitGui("policemdc:openvehiclerecord", vehicle.Id, catalogVehicle.DisplayName, catalogVehicle.DisplayClass, vehicle.NumberplateText, ownerName, nodes);
    }

    public async Task OpenBankAccountRecord(ServerPlayer player, string targetBankAccountId)
    {
        var nodes = await _mdcNoteService.Where(r => r.TargetModelId == targetBankAccountId && r.Type == MdcSearchType.BANK_ACCOUNT);
        var bankAccount = await _bankAccountService.GetByKey(int.Parse(targetBankAccountId));
        if (bankAccount == null)
        {
            return;
        }
        
        player.EmitGui("policemdc:openbankaccountrecord", bankAccount, nodes);
    }

    public async Task OpenMailAccountRecord(ServerPlayer player, string mailAddress)
    {
        var nodes = await _mdcNoteService.Where(r => r.TargetModelId == mailAddress && r.Type == MdcSearchType.MAIL);
        var mailAccount = await _mailAccountService.GetByKey(mailAddress);
        if (mailAccount == null)
        {
            return;
        }
        
        player.EmitGui("policemdc:openmailaccountrecord", mailAccount, nodes);
    }

    public async Task OpenWeaponRecord(ServerPlayer player, string targetWeaponId)
    {
        var nodes = await _mdcNoteService.Where(r => r.TargetModelId == targetWeaponId && r.Type == MdcSearchType.WEAPON);
        var weaponModel = await _itemWeaponService.GetByKey(int.Parse(targetWeaponId));
        if (weaponModel == null)
        {
            return;
        }
        
        var ownerCharacterName = string.Empty;
        var ownerCharacter = await _characterService.GetByKey(weaponModel.InitialOwnerId);
        if (ownerCharacter != null)
        {
            ownerCharacterName = ownerCharacter.Name;
        }
        
        player.EmitGui("policemdc:openweaponrecord", weaponModel.Id, weaponModel.SerialNumber, ownerCharacterName, weaponModel.CatalogItemModel.Name, nodes);
    }

    public async Task UpdateCurrentRecord(ServerPlayer player, MdcSearchType type, string id)
    {
        switch (type)
        {
            case MdcSearchType.NAME:
                await OpenCharacterRecord(player, id);
                break;
            case MdcSearchType.NUMBER:
                await OpenPhoneRecord(player, id);
                break;
            case MdcSearchType.VEHICLE:
                await OpenVehicleRecord(player, id);
                break;
            case MdcSearchType.BANK_ACCOUNT:
                await OpenBankAccountRecord(player, id);
                break;
            case MdcSearchType.MAIL:
                await OpenMailAccountRecord(player, id);
                break;
            case MdcSearchType.WEAPON:
                await OpenWeaponRecord(player, id);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
}