using System.Collections.Generic;
using System.Threading.Tasks;
using Server.Core.Abstractions.ScriptStrategy;
using Server.DataAccessLayer.Services;
using Server.Database.Models.Mdc;

namespace Server.Modules.MDC;

public class AllergiesModule : ISingletonScript
{
    private readonly AllergiesService _allergiesService;

    public AllergiesModule(AllergiesService allergiesService)
    {
        _allergiesService = allergiesService;
    }

    public async Task Add(int targetCharacterId, string creatorName, string content)
    {
        await _allergiesService.Add(new MdcAllergyModel
        {
            CharacterModelId = targetCharacterId, CreatorCharacterName = creatorName, Content = content
        });
    }

    public async Task<MdcAllergyModel?> GetById(int id)
    {
        return await _allergiesService.GetByKey(id);
    }

    public async Task Remove(MdcAllergyModel mdcMedicalEntryModel)
    {
        await _allergiesService.Remove(mdcMedicalEntryModel);
    }

    public async Task<List<MdcAllergyModel>> GetByCharacterId(int targetCharacterId)
    {
        return await _allergiesService.Where(m => m.CharacterModelId == targetCharacterId);
    }
}