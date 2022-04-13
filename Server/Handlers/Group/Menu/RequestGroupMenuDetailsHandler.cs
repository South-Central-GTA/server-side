using System.Collections.Generic;
using System.Linq;
using AltV.Net;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Models;
using Server.DataAccessLayer.Services;

namespace Server.Handlers.Group;

public class RequestGroupMenuDetailsHandler : ISingletonScript
{
    private readonly GroupService _groupService;
    private readonly HouseService _houseService;

    public RequestGroupMenuDetailsHandler(
        GroupService groupService,
        HouseService houseService)
    {
        _groupService = groupService;
        _houseService = houseService;

        AltAsync.OnClient<ServerPlayer, int>("groupmenu:requestdetails", OnRequestDetails);
    }

    private async void OnRequestDetails(ServerPlayer player, int groupId)
    {
        if (!player.Exists)
        {
            return;
        }

        var onlineGroupPlayers = new List<PlayerInformationData>();
        var closeToBase = false;

        if (groupId != -1)
        {
            var group = await _groupService.GetByKey(groupId);
            if (group != null && group.Members.Any(m => m.CharacterModelId == player.CharacterModel.Id))
            {
                var house = await _houseService.Find(h => h.GroupModelId == group.Id);
                if (house != null)
                {
                    closeToBase = player.Position.Distance(house.Position) <= 20;
                }

                var memberCharacterIds = group.Members.Select(m => m.CharacterModelId);
                var players = Alt.GetAllPlayers()
                                 .GetAllServerPlayers()
                                 .Where(p => memberCharacterIds.Contains(p.CharacterModel.Id));

                onlineGroupPlayers = players
                                     .Select(target => new PlayerInformationData
                                     {
                                         Id = target.Id, CharacterName = target.CharacterModel.Name
                                     }).ToList();
            }
        }

        player.EmitGui("groupmenu:senddetails", closeToBase, onlineGroupPlayers);
    }
}