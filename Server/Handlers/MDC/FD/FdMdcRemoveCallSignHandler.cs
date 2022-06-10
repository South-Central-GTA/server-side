using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.MDC;

namespace Server.Handlers.MDC.FD;

public class FdMdcRemoveCallSignHandler : ISingletonScript
{
    private readonly FireMdcModule _fireMdcModule;
    private readonly GroupFactionService _groupFactionService;

    public FdMdcRemoveCallSignHandler(GroupFactionService groupFactionService, FireMdcModule fireMdcModule)
    {
        _groupFactionService = groupFactionService;

        _fireMdcModule = fireMdcModule;

        AltAsync.OnClient<ServerPlayer>("firemdc:removecallsign", OnRemoveCallSign);
    }

    private async void OnRemoveCallSign(ServerPlayer player)
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

        await _fireMdcModule.CallSign.RemoveCallSign(player);
    }
}