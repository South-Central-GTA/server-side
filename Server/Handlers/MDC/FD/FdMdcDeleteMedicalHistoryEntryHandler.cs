using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.MDC;

namespace Server.Handlers.MDC.FD;

public class FdMdcDeleteMedicalHistoryEntryHandler : ISingletonScript
{
    private readonly FireMdcModule _fireMdcModule;
    private readonly FactionGroupService _factionGroupService;

    private readonly MedicalHistoryModule _medicalHistoryModule;

    public FdMdcDeleteMedicalHistoryEntryHandler(FactionGroupService factionGroupService,
        MedicalHistoryModule medicalHistoryModule, FireMdcModule fireMdcModule)
    {
        _factionGroupService = factionGroupService;

        _medicalHistoryModule = medicalHistoryModule;
        _fireMdcModule = fireMdcModule;

        AltAsync.OnClient<ServerPlayer, int>("firemdc:deletemedicalhistory", OnDeleteMedicalHistoryEntry);
    }

    private async void OnDeleteMedicalHistoryEntry(ServerPlayer player, int entryId)
    {
        if (!player.Exists)
        {
            return;
        }

        var factionGroup = await _factionGroupService.GetByCharacter(player.CharacterModel.Id);
        if (factionGroup is not { FactionType: FactionType.FIRE_DEPARTMENT })
        {
            return;
        }

        var allergyModel = await _medicalHistoryModule.GetById(entryId);
        if (allergyModel == null)
        {
            return;
        }

        await _medicalHistoryModule.Remove(allergyModel);

        await _fireMdcModule.OpenPatientRecords(player, allergyModel.CharacterModelId);
    }
}