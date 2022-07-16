using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.Group;
using Server.Modules.MDC;

namespace Server.Handlers.MDC.PD;

public class PdMdcDeleteEmergencyCallHandler : ISingletonScript
{
    private readonly EmergencyCallService _emergencyCallService;
    private readonly FactionGroupService _factionGroupService;
    private readonly GroupModule _groupModule;

    private readonly PoliceMdcModule _policeMdcModule;

    public PdMdcDeleteEmergencyCallHandler(EmergencyCallService emergencyCallService,
        FactionGroupService factionGroupService, PoliceMdcModule policeMdcModule, GroupModule groupModule)
    {
        _emergencyCallService = emergencyCallService;
        _factionGroupService = factionGroupService;

        _policeMdcModule = policeMdcModule;
        _groupModule = groupModule;

        AltAsync.OnClient<ServerPlayer, int>("policemdc:deleteemergencycall", OnDeleteEmergencyCall);
    }

    private async void OnDeleteEmergencyCall(ServerPlayer player, int emergencyCallModelId)
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

        var emergencyCallModel = await _emergencyCallService.GetByKey(emergencyCallModelId);
        if (emergencyCallModel == null)
        {
            return;
        }

        await _emergencyCallService.Remove(emergencyCallModel);
        await _policeMdcModule.UpdateEmergencyCallsUi(player);
    }
}