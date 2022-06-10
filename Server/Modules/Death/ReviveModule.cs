using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;

namespace Server.Modules.Death;

public class ReviveModule : ISingletonScript
{
    private readonly CharacterService _characterService;
    private readonly GroupFactionService _groupFactionService;

    public ReviveModule(CharacterService characterService, GroupFactionService groupFactionService)
    {
        _characterService = characterService;
        _groupFactionService = groupFactionService;
    }

    public async Task RevivePlayer(ServerPlayer player, ushort playerId)
    {
        if (!player.Exists)
        {
            return;
        }

        var factionGroup = await _groupFactionService.GetFactionByCharacter(player.CharacterModel.Id);
        if (factionGroup is null || factionGroup.FactionType != FactionType.FIRE_DEPARTMENT)
        {
            return;
        }

        var playerToRevive = Alt.GetAllPlayers().FindPlayerById(playerId);
        if (playerToRevive != null)
        {
            await ExecuteRevive(playerToRevive);
        }
    }

    public async Task AutoRevivePlayer(ServerPlayer player, Position position)
    {
        await ExecuteRevive(player, position);
    }

    private async Task ExecuteRevive(ServerPlayer player, Position? position = null)
    {
        if (!player.Exists)
        {
            return;
        }

        if (player.CharacterModel.DeathState != DeathState.DEAD)
        {
            return;
        }

        player.CharacterModel.DeathState = DeathState.ALIVE;
        player.SetSyncedMetaData("DEATH_STATE", player.CharacterModel.DeathState);

        await _characterService.Update(player.CharacterModel);

        await player.SetInvincibleAsync(false);
        await player.SetPositionAsync(position ?? player.Position);

        player.ClearTimer("player_respawn");
        player.EmitLocked("death:revive");
        player.Invincible = false;
    }
}