using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Modules.Group;

namespace Server.Handlers.Group;

public class KickGroupMemberHandler : ISingletonScript
{
    private readonly GroupMemberService _groupMemberService;
    private readonly GroupModule _groupModule;

    public KickGroupMemberHandler(GroupMemberService groupMemberService, GroupModule groupModule)
    {
        _groupMemberService = groupMemberService;
        _groupModule = groupModule;

        AltAsync.OnClient<ServerPlayer, int, int>("group:kickmember", OnKickMember);
    }

    private async void OnKickMember(ServerPlayer player, int groupId, int characterId)
    {
        if (!player.Exists)
        {
            return;
        }

        var member =
            await _groupMemberService.Find(m => m.GroupModelId == groupId && m.CharacterModelId == characterId);
        if (member == null)
        {
            return;
        }

        await _groupModule.Kick(player, member);
    }
}