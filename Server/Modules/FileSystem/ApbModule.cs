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
    private readonly GroupFactionService _groupFactionService;

    public ApbModule(GroupFactionService groupFactionService, BaseMdcModule baseMdcModule)
    {
        _groupFactionService = groupFactionService;
        _baseMdcModule = baseMdcModule;
    }

    public async Task UpdateUi(int groupId)
    {
        var factionGroupModel = await _groupFactionService.GetByKey(groupId);
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