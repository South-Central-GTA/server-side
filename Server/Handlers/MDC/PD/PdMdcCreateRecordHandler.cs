using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.MDC;

namespace Server.Handlers.MDC.PD;

public class PdMdcCreateRecordHandler : ISingletonScript
{
    private readonly CriminalRecordModule _criminalRecordModule;
    private readonly FactionGroupService _factionGroupService;
    private readonly PoliceMdcModule _policeMdcModule;

    public PdMdcCreateRecordHandler(FactionGroupService factionGroupService, CriminalRecordModule criminalRecordModule,
        PoliceMdcModule policeMdcModule)
    {
        _factionGroupService = factionGroupService;

        _criminalRecordModule = criminalRecordModule;
        _policeMdcModule = policeMdcModule;

        AltAsync.OnClient<ServerPlayer, int, string>("policemdc:createrecord", OnCreateRecord);
    }

    private async void OnCreateRecord(ServerPlayer player, int characterId, string input)
    {
        if (!player.Exists)
        {
            return;
        }

        var factionGroup = await _factionGroupService.GetByCharacter(player.CharacterModel.Id);
        if (factionGroup is not { FactionType: FactionType.POLICE_DEPARTMENT })
        {
            return;
        }

        await _criminalRecordModule.Add(characterId, player.CharacterModel.Name, input);

        await _policeMdcModule.OpenCharacterRecord(player, characterId.ToString());
    }
}