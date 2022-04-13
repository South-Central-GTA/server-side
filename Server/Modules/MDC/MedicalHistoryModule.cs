using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Server.Core.Abstractions.ScriptStrategy;
using Server.DataAccessLayer.Services;
using Server.Database.Models.Mdc;

namespace Server.Modules.MDC;

public class MedicalHistoryModule
    : ISingletonScript
{
    private readonly MedicalHistoryService _medicalHistoryService;

    public MedicalHistoryModule(MedicalHistoryService medicalHistoryService)
    {
        _medicalHistoryService = medicalHistoryService;
    }

    public async Task Add(int targetCharacterId, string creatorName, string content)
    {
        await _medicalHistoryService.Add(new MdcMedicalEntryModel()
        {
            CharacterModelId = targetCharacterId,
            CreatorCharacterName = creatorName,
            Content = content
        });
    }

    public async Task<MdcMedicalEntryModel?> GetById(int id)
    {
        return await _medicalHistoryService.GetByKey(id);
    }

    public async Task Remove(MdcMedicalEntryModel mdcMedicalEntryModel)
    {
        await _medicalHistoryService.Remove(mdcMedicalEntryModel);
    }

    public async Task<List<MdcMedicalEntryModel>> GetByCharacterId(int targetCharacterId)
    {
        return await _medicalHistoryService.Where(m => m.CharacterModelId == targetCharacterId);
    }
}