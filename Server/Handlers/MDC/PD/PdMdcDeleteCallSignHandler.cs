using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.Group;
using Server.Modules.MDC;

namespace Server.Handlers.MDC.PD;

public class PdMdcDeleteCallSignHandler : ISingletonScript
{
    private readonly GroupFactionService _groupFactionService;

    private readonly PoliceMdcModule _policeMdcModule;
    private readonly GroupModule _groupModule;

    public PdMdcDeleteCallSignHandler(
        GroupFactionService groupFactionService,
        PoliceMdcModule policeMdcModule,
        GroupModule groupModule)
    {
        _groupFactionService = groupFactionService;

        _policeMdcModule = policeMdcModule;
        _groupModule = groupModule;

        AltAsync.OnClient<ServerPlayer, string>("policemdc:deletecallsign", OnDeleteEmergencyCall);
    }

    private async void OnDeleteEmergencyCall(ServerPlayer player, string callSign)
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

        await _policeMdcModule.CallSign.DeleteCallSign(player, callSign);
    }
}