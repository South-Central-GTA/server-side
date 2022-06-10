using System.Threading.Tasks;
using Server.Core.Abstractions.ScriptStrategy;
using Server.DataAccessLayer.Services;
using Server.Database.Models.Mdc;

namespace Server.Modules.MDC;

public class CriminalRecordModule : ISingletonScript
{
    private readonly CriminalRecordService _criminalRecordService;

    public CriminalRecordModule(CriminalRecordService criminalRecordService)
    {
        _criminalRecordService = criminalRecordService;
    }

    public async Task Add(int targetCharacterId, string creatorName, string reason)
    {
        await _criminalRecordService.Add(new CriminalRecordModel
        {
            CharacterModelId = targetCharacterId, CreatorCharacterName = creatorName, Reason = reason
        });
    }

    public async Task<CriminalRecordModel?> GetById(int id)
    {
        return await _criminalRecordService.GetByKey(id);
    }

    public async Task Remove(CriminalRecordModel criminalRecordModel)
    {
        await _criminalRecordService.Remove(criminalRecordModel);
    }
}