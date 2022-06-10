using System.Linq;
using AltV.Net;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Models;
using Server.Database.Enums;

namespace Server.Handlers.Admin;

public class PlayerCatalogHandler : ISingletonScript
{
    public PlayerCatalogHandler()
    {
        AltAsync.OnClient<ServerPlayer>("playercatalog:open", OnOpenPlayerCatalog);
    }

    private async void OnOpenPlayerCatalog(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.AccountModel.Permission.HasFlag(Permission.STAFF))
        {
            return;
        }

        var players = Alt.GetAllPlayers().GetAllServerPlayers();
        if (players == null)
        {
            return;
        }

        var playerInformationData = players.Select(target => new PlayerInformationData
        {
            Id = target.Id,
            AccountId = target.AccountModel.SocialClubId,
            AccountName = target.AccountName,
            CharacterId = target.CharacterModel.Id,
            CharacterName = target.CharacterModel.Name,
            DiscordId = target.DiscordId
        }).ToList();

        player.EmitGui("playercatalog:open", playerInformationData);
    }
}