using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.Group;
using Server.Modules.MDC;

namespace Server.Handlers.MDC.PD;

public class PdMdcCreateRecordHandler : ISingletonScript
{
    private readonly GroupFactionService _groupFactionService;
    
    private readonly CriminalRecordModule _criminalRecordModule;
    private readonly PoliceMdcModule _policeMdcModule;
 
    public PdMdcCreateRecordHandler(
        GroupFactionService groupFactionService,

        CriminalRecordModule criminalRecordModule,
        PoliceMdcModule policeMdcModule) 
    {
        _groupFactionService = groupFactionService;
        
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

        var factionGroup = await _groupFactionService.GetFactionByCharacter(player.CharacterModel.Id);
        if (factionGroup is not { FactionType: FactionType.POLICE_DEPARTMENT })
        {
            return;
        }

        await _criminalRecordModule.Add(characterId, player.CharacterModel.Name, input);
        
        await _policeMdcModule.OpenCharacterRecord(player, characterId.ToString());
    }
}