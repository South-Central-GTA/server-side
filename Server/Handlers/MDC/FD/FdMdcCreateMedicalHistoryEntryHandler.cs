using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.MDC;

namespace Server.Handlers.MDC.FD;

public class FdMdcCreateMedicalHistoryEntryHandler : ISingletonScript
{
    private readonly FireMdcModule _fireMdcModule;
    private readonly GroupFactionService _groupFactionService;

    private readonly MedicalHistoryModule _medicalHistoryModule;

    public FdMdcCreateMedicalHistoryEntryHandler(GroupFactionService groupFactionService,
        MedicalHistoryModule medicalHistoryModule, FireMdcModule fireMdcModule)
    {
        _groupFactionService = groupFactionService;

        _medicalHistoryModule = medicalHistoryModule;
        _fireMdcModule = fireMdcModule;

        AltAsync.OnClient<ServerPlayer, int, string>("firemdc:createmedicalhistory", OnCreateMedicalHistory);
    }

    private async void OnCreateMedicalHistory(ServerPlayer player, int characterId, string input)
    {
        if (!player.Exists)
        {
            return;
        }

        var factionGroup = await _groupFactionService.GetFactionByCharacter(player.CharacterModel.Id);
        if (factionGroup is not { FactionType: FactionType.FIRE_DEPARTMENT })
        {
            return;
        }

        await _medicalHistoryModule.Add(characterId, player.CharacterModel.Name, input);

        await _fireMdcModule.OpenPatientRecords(player, characterId);
    }
}