using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Mdc;

namespace Server.Modules.MDC;

public class FireMdcModule
    : ISingletonScript
{
    public CallSign CallSign { get; }

    private readonly EmergencyCallService _emergencyCallService;
    private readonly GroupFactionService _groupFactionService;
    private readonly CharacterService _characterService;
    private readonly HouseService _houseService;
    private readonly ItemPhoneService _itemPhoneService;

    private readonly MedicalHistoryModule _medicalHistoryModule;
    private readonly AllergiesModule _allergiesModule;

    public FireMdcModule(
        GroupFactionService groupFactionService,
        EmergencyCallService emergencyCallService,
        CharacterService characterService,
        HouseService houseService,
        ItemPhoneService itemPhoneService,
        MedicalHistoryModule medicalHistoryModule,
        AllergiesModule allergiesModule)
    {
        CallSign = new CallSign(groupFactionService);
        _groupFactionService = groupFactionService;
        _emergencyCallService = emergencyCallService;
        _characterService = characterService;
        _houseService = houseService;
        _itemPhoneService = itemPhoneService;

        _medicalHistoryModule = medicalHistoryModule;
        _allergiesModule = allergiesModule;
    }

    public async Task<List<EmergencyCallModel>> GetEmergencyCalls()
    {
        var emergencyCalls = await _emergencyCallService.GetAll();
        return emergencyCalls.Where(e => e.FactionType == FactionType.FIRE_DEPARTMENT).ToList();
    }

    public async Task UpdateEmergencyCallsUi(ServerPlayer player)
    {
        var factionGroup = await _groupFactionService.GetFactionByCharacter(player.CharacterModel.Id);
        if (factionGroup == null)
        {
            return;
        }

        foreach (var target in factionGroup.Members
                                           .Select(groupMember =>
                                                       Alt.GetAllPlayers()
                                                          .FindPlayerByCharacterId(groupMember.CharacterModelId))
                                           .Where(serverPlayer => serverPlayer != null))
        {
            target.EmitGui("firepolicemdc:updateemergencycalls", await GetEmergencyCalls());
        }
    }

    public async Task OpenPatientRecords(ServerPlayer player, int targetCharacterId)
    {
        var character = await _characterService.GetByKey(targetCharacterId);
        if (character == null)
        {
            return;
        }

        var medicalHistory = await _medicalHistoryModule.GetByCharacterId(targetCharacterId);
        var allergies = await _allergiesModule.GetByCharacterId(targetCharacterId);

        var houses = await _houseService.Where(h => h.CharacterModelId == character.Id);
        var phoneModels = await _itemPhoneService.Where(p => p.InitialOwnerId == character.Id);
        var phoneNumbers = phoneModels.Select(p => p.PhoneNumber).ToList();

        player.EmitLocked("firemdc:openpatientrecord", character, houses, phoneNumbers, medicalHistory, allergies);
    }
}