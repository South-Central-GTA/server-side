using System.Threading.Tasks;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Modules.MDC;

namespace Server.Handlers.MDC.FD;

public class FdMdcOpenPageHandler : ISingletonScript
{
    private readonly BaseMdcModule _baseMdcModule;
    private readonly FireMdcModule _fireMdcModule;
 
    public FdMdcOpenPageHandler(
        BaseMdcModule baseMdcModule, 
        FireMdcModule fireMdcModule)
    {
        _baseMdcModule = baseMdcModule;
        _fireMdcModule = fireMdcModule;

        AltAsync.OnClient<ServerPlayer, int>("firemdc:openpage", OnOpenPage);
    }

    private async void OnOpenPage(ServerPlayer player, int pageId)
    {
        if (!player.Exists)
        {
            return;
        }

        await _baseMdcModule.UpdateOperatorPermissionUi(player);

        switch (pageId)
        {
            case 0:
                await OpenHomeScreen(player);
                break;
        }
    }

    private async Task OpenHomeScreen(ServerPlayer player)
    {
        player.EmitGui("firemdc:openhomescreen", 
                       await _fireMdcModule.GetEmergencyCalls(), 
                       _fireMdcModule.CallSign.GetCallSigns(), 
                       _fireMdcModule.CallSign.HasCallSign(player.CharacterModel));
    }
}