using System.Linq;
using System.Threading.Tasks;
using AltV.Net;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Extensions;
using Server.DataAccessLayer.Services;
using Server.Modules.MDC;

namespace Server.Modules.FileSystem;

public class ApbModule : ISingletonScript
{
    private readonly BaseMdcModule _baseMdcModule;
    private readonly FactionGroupService _factionGroupService;

    public ApbModule(FactionGroupService factionGroupService, BaseMdcModule baseMdcModule)
    {
        _factionGroupService = factionGroupService;
        _baseMdcModule = baseMdcModule;
    }

    public async Task UpdateUi(int groupId)
    {
        var factionGroupModel = await _factionGroupService.GetByKey(groupId);
        if (factionGroupModel == null)
        {
            return;
        }

        foreach (var serverPlayer in factionGroupModel.Members
                     .Select(groupMember => Alt.GetAllPlayers().FindPlayerByCharacterId(groupMember.CharacterModelId))
                     .Where(serverPlayer => serverPlayer != null))
        {
            if (!serverPlayer.HasData("APB_SCREEN_OPEN"))
            {
                continue;
            }

            serverPlayer.EmitGui("policemdc:openapbscreen",
                await _baseMdcModule.GetBulletInEntries(factionGroupModel.FactionType));
        }
    }
}