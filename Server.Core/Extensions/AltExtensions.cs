using System;
using System.Collections.Generic;
using System.Linq;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Server.Core.Entities;
using Server.Data.Enums;

namespace Server.Core.Extensions;

public static class AltExtensions
{
    public static List<ServerPlayer> GetAllServerPlayers(this ICollection<IPlayer> players)
    {
        return players.ToList().ConvertAll(x => (ServerPlayer)x).Where(p => p.Exists && p.IsLoggedIn && p.IsSpawned).ToList();
    }

    public static List<ServerPlayer> FindPlayerByName(this ICollection<IPlayer> players, string searchName)
    {
        return GetAllServerPlayers(players).Where(player => player.AccountName.ToLower().Contains(searchName.ToLower())).ToList();
    }

    public static ServerPlayer? FindPlayerByCharacterId(this ICollection<IPlayer> players, int characterId)
    {
        return GetAllServerPlayers(players).Find(player => player.IsSpawned && player.CharacterModel.Id == characterId);
    }

    public static ServerPlayer? FindPlayerById(this ICollection<IPlayer> players, int playerId)
    {
        return GetAllServerPlayers(players).Find(player => player.IsSpawned && player.Id == playerId);
    }

    public static ServerPlayer? GetPlayerById(this ICollection<IPlayer> players, ServerPlayer player, string expectedPlayerId)
    {
        if (!int.TryParse(expectedPlayerId, out var playerId))
        {
            player.SendNotification("Bitte gebe ein richtige Zahl als Id an.", NotificationType.ERROR);
            return null;
        }

        var target = GetAllServerPlayers(players).Find(p => p.Id == playerId);
        if (target is not { Exists: true })
        {
            player.SendNotification("Kein Spieler gefunden.", NotificationType.ERROR);
            return null;
        }

        return target;
    }

    public static ServerPlayer? GetPlayerById(this ICollection<IPlayer> players, ServerPlayer player, int playerId, bool sendNotification = true)
    {
        var target = GetAllServerPlayers(players).Find(p => p.Id == playerId);
        if (target is not { Exists: true } && sendNotification)
        {
            player.SendNotification("Kein Spieler gefunden.", NotificationType.ERROR);
            return null;
        }

        return target;
    }

    public static ServerPlayer? FindPlayerBySocialId(this ICollection<IPlayer> players, ulong socialId)
    {
        return GetAllServerPlayers(players).Find(player => player.SocialClubId == socialId);
    }

    public static ServerPlayer? FindPlayerByDiscordId(this ICollection<IPlayer> players, ulong discordId)
    {
        return GetAllServerPlayers(players).Find(player => player.DiscordId == discordId);
    }

    public static List<ServerPlayer> GetByRange(this ICollection<IPlayer> players, Position position, float radius)
    {
        return GetAllServerPlayers(players).FindAll(p => p.Position.Distance(position) <= radius);
    }

    public static List<ServerPlayer> Where(this ICollection<IPlayer> players, Predicate<ServerPlayer> match)
    {
        return GetAllServerPlayers(players).FindAll(match);
    }

    public static List<ServerVehicle> GetAllVehicles(this ICollection<IVehicle> vehicles)
    {
        return vehicles.Where(v => v.Exists).ToList().ConvertAll(x => (ServerVehicle)x);
    }

    public static ServerVehicle? FindByDbId(this ICollection<IVehicle> vehicles, int dbId)
    {
        return GetAllVehicles(vehicles).FirstOrDefault(v => v.DbEntity?.Id == dbId);
    }

    public static ServerVehicle? GetClosest(this ICollection<IVehicle> vehicles, Position position, float radius = 5f)
    {
        var closestDistance = float.MaxValue;
        ServerVehicle closestVehicle = null;
        foreach (var v in GetAllVehicles(vehicles))
        {
            var distance = v.Position.Distance(position);
            if (distance <= radius && (distance < closestDistance) && v.Exists)
            {
                closestDistance = distance;
                closestVehicle = v;
            }
        }

        return closestVehicle;
    }
}