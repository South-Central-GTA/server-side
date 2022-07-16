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
    private readonly FactionGroupService _factionGroupService;
    private readonly GroupModule _groupModule;

    private readonly PoliceMdcModule _policeMdcModule;

    public PdMdcDeleteCallSignHandler(FactionGroupService factionGroupService, PoliceMdcModule policeMdcModule,
        GroupModule groupModule)
    {
        _factionGroupService = factionGroupService;

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

        var factionGroup = await _factionGroupService.GetByCharacter(player.CharacterModel.Id);
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