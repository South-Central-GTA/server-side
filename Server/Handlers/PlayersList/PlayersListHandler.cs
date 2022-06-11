using System.Linq;
using AltV.Net;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Models;

namespace Server.Handlers.PlayersList;

public class PlayerListsHandler : ISingletonScript
{
    public PlayerListsHandler()
    {
        AltAsync.OnClient<ServerPlayer>("playerslist:requestmenu", OnRequestMenu);
    }

    private async void OnRequestMenu(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        var players = Alt.GetAllPlayers().GetAllServerPlayers();
        var playerInformationDatas = players.Select(target => new PlayerInformationData
        {
            Id = target.Id, CharacterName = target.CharacterModel.Name
        }).ToList();

        player.EmitLocked("playerslist:show", playerInformationDatas);
    }
}