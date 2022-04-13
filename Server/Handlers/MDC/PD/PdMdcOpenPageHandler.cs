using System.Threading.Tasks;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.DataAccessLayer.Services;
using Server.Modules.FileSystem;
using Server.Modules.MDC;

namespace Server.Handlers.MDC.PD;

public class PdMdcOpenPageHandler : ISingletonScript
{
    private readonly GroupFactionService _groupFactionService;
    
    private readonly PoliceMdcModule _policeMdcModule;
    private readonly BaseMdcModule _baseMdcModule;
    private readonly FileModule _fileModule;

    public PdMdcOpenPageHandler(
        GroupFactionService groupFactionService,
        
        PoliceMdcModule policeMdcModule, 
        BaseMdcModule baseMdcModule, 
        FileModule fileModule)
    {
        _groupFactionService = groupFactionService;
        
        _policeMdcModule = policeMdcModule;
        _baseMdcModule = baseMdcModule;
        _fileModule = fileModule;

        AltAsync.OnClient<ServerPlayer, int>("policemdc:openpage", OnOpenPage);
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
            case 4:
                await OpenFileScreen(player);
                break;
        }
    }

    private async Task OpenHomeScreen(ServerPlayer player)
    {
        player.EmitGui("policemdc:openhomescreen",  
                       await _policeMdcModule.GetEmergencyCalls(), 
                       _policeMdcModule.CallSign.GetCallSigns(), 
                       _policeMdcModule.CallSign.HasCallSign(player.CharacterModel));
    }

    private async Task OpenFileScreen(ServerPlayer player)
    {
        var factionGroup = await _groupFactionService.GetFactionByCharacter(player.CharacterModel.Id);
        if (factionGroup == null)
        {
            return;
        }

        await _fileModule.OpenFileSystem(player, factionGroup.Id);
    }
}