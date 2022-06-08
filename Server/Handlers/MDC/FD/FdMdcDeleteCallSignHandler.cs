using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Group;
using Server.Modules.Group;
using Server.Modules.MDC;

namespace Server.Handlers.MDC.FD;

public class FdMdcDeleteCallSignHandler : ISingletonScript
{
    private readonly GroupFactionService _groupFactionService;

    private readonly FireMdcModule _fireMdcModule;
    private readonly GroupModule _groupModule;

    public FdMdcDeleteCallSignHandler(
        GroupFactionService groupFactionService,
        FireMdcModule fireMdcModule,
        GroupModule groupModule)
    {
        _groupFactionService = groupFactionService;

        _fireMdcModule = fireMdcModule;
        _groupModule = groupModule;

        AltAsync.OnClient<ServerPlayer, string>("firemdc:deletecallsign", OnDeleteEmergencyCall);
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

        await _fireMdcModule.CallSign.DeleteCallSign(player, callSign);
    }
}