using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.Group;
using Server.Modules.MDC;

namespace Server.Handlers.MDC.PD;

public class PdMdcDeleteCriminalRecord : ISingletonScript
{
    private readonly GroupFactionService _groupFactionService;
    
    private readonly PoliceMdcModule _policeMdcModule;
    private readonly CriminalRecordModule _criminalRecordModule;
    private readonly GroupModule _groupModule;
 
    public PdMdcDeleteCriminalRecord(
        GroupFactionService groupFactionService,
        PoliceMdcModule policeMdcModule,
        CriminalRecordModule criminalRecordModule, 
        GroupModule groupModule)
    {
        _groupFactionService = groupFactionService;
        _policeMdcModule = policeMdcModule;
        _criminalRecordModule = criminalRecordModule;
        _groupModule = groupModule;

        AltAsync.OnClient<ServerPlayer, int>("policemdc:deleterecord", OnDeleteNote);
    }

    private async void OnDeleteNote(ServerPlayer player, int id)
    {
        if (!player.Exists)
        {
            return;
        }
        
        var factionGroup = await _groupFactionService.GetFactionByCharacter(player.CharacterModel.Id);
        if (factionGroup == null)
        {
            return;
        }

        if (!await _groupModule.HasPermission(player.CharacterModel.Id, factionGroup.Id, GroupPermission.MDC_OPERATOR))
        {
            return;
        }
        
        var criminalRecord = await _criminalRecordModule.GetById(id);
        if (criminalRecord == null)
        {
            return;
        }
        
        await _criminalRecordModule.Remove(criminalRecord);
        
        await _policeMdcModule.OpenCharacterRecord(player, criminalRecord.CharacterModelId.ToString());
    }
}