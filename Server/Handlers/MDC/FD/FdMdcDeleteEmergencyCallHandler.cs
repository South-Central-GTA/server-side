using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.Group;
using Server.Modules.MDC;

namespace Server.Handlers.MDC.FD;

public class FdMdcDeleteEmergencyCallHandler : ISingletonScript
{
    private readonly EmergencyCallService _emergencyCallService;

    private readonly FireMdcModule _fireMdcModule;
    private readonly FactionGroupService _factionGroupService;
    private readonly GroupModule _groupModule;

    public FdMdcDeleteEmergencyCallHandler(EmergencyCallService emergencyCallService,
        FactionGroupService factionGroupService, FireMdcModule fireMdcModule, GroupModule groupModule)
    {
        _emergencyCallService = emergencyCallService;
        _factionGroupService = factionGroupService;

        _fireMdcModule = fireMdcModule;
        _groupModule = groupModule;

        AltAsync.OnClient<ServerPlayer, int>("firemdc:deleteemergencycall", OnDeleteEmergencyCall);
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
        await _fireMdcModule.UpdateEmergencyCallsUi(player);
    }
}