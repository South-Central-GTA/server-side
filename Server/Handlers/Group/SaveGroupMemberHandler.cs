using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;

namespace Server.Handlers.Group;

public class SaveGroupMemberHandler : ISingletonScript
{
    private readonly GroupMemberService _groupMemberService;

    public SaveGroupMemberHandler(GroupMemberService groupMemberService)
    {
        ;
        _groupMemberService = groupMemberService;

        AltAsync.OnClient<ServerPlayer, int, int, uint, uint>("group:savemember", OnSaveMember);
    }

    private async void OnSaveMember(ServerPlayer player, int groupId, int characterId, uint level, uint salary)
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

        member.RankLevel = level;
        member.Salary = salary;

        await _groupMemberService.Update(member);
    }
}