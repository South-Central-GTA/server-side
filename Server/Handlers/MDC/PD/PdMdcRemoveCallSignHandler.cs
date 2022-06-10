using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.MDC;

namespace Server.Handlers.MDC.PD;

public class PdMdcRemoveCallSignHandler : ISingletonScript
{
    private readonly GroupFactionService _groupFactionService;
    private readonly PoliceMdcModule _policeMdcModule;

    public PdMdcRemoveCallSignHandler(GroupFactionService groupFactionService, PoliceMdcModule policeMdcModule)
    {
        _policeMdcModule = policeMdcModule;
        _groupFactionService = groupFactionService;

        AltAsync.OnClient<ServerPlayer>("policemdc:removecallsign", OnRemoveCallSign);
    }

    private async void OnRemoveCallSign(ServerPlayer player)
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

        await _policeMdcModule.CallSign.RemoveCallSign(player);
    }
}