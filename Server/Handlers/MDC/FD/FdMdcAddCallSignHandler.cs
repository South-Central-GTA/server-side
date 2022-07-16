using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.MDC;

namespace Server.Handlers.MDC.FD;

public class FdMdcAddCallSignHandler : ISingletonScript
{
    private readonly FireMdcModule _fireMdcModule;
    private readonly FactionGroupService _factionGroupService;

    public FdMdcAddCallSignHandler(FactionGroupService factionGroupService, FireMdcModule fireMdcModule)
    {
        _factionGroupService = factionGroupService;

        _fireMdcModule = fireMdcModule;

        AltAsync.OnClient<ServerPlayer, string>("firemdc:addcallsign", OnAddCallSign);
    }

    private async void OnAddCallSign(ServerPlayer player, string callSign)
    {
        if (!player.Exists)
        {
            return;
        }

        var factionGroup = await _factionGroupService.GetByCharacter(player.CharacterModel.Id);
        if (factionGroup is not { FactionType: FactionType.FIRE_DEPARTMENT })
        {
            return;
        }

        await _fireMdcModule.CallSign.AddCallSign(player, callSign);
    }
}