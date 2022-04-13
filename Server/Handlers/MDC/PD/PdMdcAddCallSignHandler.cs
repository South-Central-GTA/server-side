using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.MDC;

namespace Server.Handlers.MDC.PD;

public class PdMdcAddCallSignHandler : ISingletonScript
{
    private readonly GroupFactionService _groupFactionService;
    
    private readonly PoliceMdcModule _policeMdcModule;
 
    public PdMdcAddCallSignHandler(
        GroupFactionService groupFactionService,
        
        PoliceMdcModule policeMdcModule)
    {
        _groupFactionService = groupFactionService;
        
        _policeMdcModule = policeMdcModule;

        AltAsync.OnClient<ServerPlayer, string>("policemdc:addcallsign", OnAddCallSign);
    }

    private async void OnAddCallSign(ServerPlayer player, string callSign)
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
        
        await _policeMdcModule.CallSign.AddCallSign(player, callSign);
    }
}