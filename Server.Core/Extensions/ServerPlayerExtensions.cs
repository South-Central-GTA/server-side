using System.Collections.Generic;
using System.Linq;
using System.Timers;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using Server.Core.Entities;
using Server.Data.Enums;
using Server.Data.Models;
using Server.Database.Enums;

namespace Server.Core.Extensions;

public static class ServerPlayerExtensions
{
    public static void EmitGui(this ServerPlayer player, string eventName, params object[] args)
    {
        player.EmitLocked("webview:emit", eventName, args);
    }

    public static void SendNotification(this ServerPlayer player, string text, NotificationType type)
    {
        player.EmitLocked("notification:send", new Notification(type, text));
    }

    public static void SetWaypoint(this ServerPlayer player, Position position, int color, int sprite)
    {
        player.EmitLocked("waypoint:set", position.X, position.Y, position.Z, color, sprite);
    }

    public static void ClearWaypoint(this ServerPlayer player)
    {
        player.EmitLocked("waypoint:clear");
    }

    public static void SendSubtitle(this ServerPlayer player, string message, int durationInMs)
    {
        player.EmitLocked("subtitle:draw", message, durationInMs);
    }

    public static void ClearSubtitle(this ServerPlayer player)
    {
        player.EmitLocked("subtitle:clear");
    }

    public static List<ServerPlayer> GetPlayersAround(this ServerPlayer player, float radius, bool withHimself = false)
    {
        var nearPlayers = new List<ServerPlayer>();

        foreach (var p in Alt.GetAllPlayers())
        {
            if (p.Position.Distance(player.Position) <= radius && p is ServerPlayer serverPlayer)
            {
                nearPlayers.Add(serverPlayer);

                if (!withHimself)
                {
                    nearPlayers.Remove(player);
                }
            }
        }

        return nearPlayers;
    }

    public static void SetUniqueDimension(this ServerPlayer player)
    {
        var dim = player.Id + 100;
        player.Dimension = dim;
    }

    public static void UpdateMoneyUi(this ServerPlayer player)
    {
        player.EmitLocked("hud:setmoney",
            player.CharacterModel.InventoryModel.Items.Where(i => i.CatalogItemModelId == ItemCatalogIds.DOLLAR)
                .Sum(i => i.Amount));
    }

    public static void CreateDialog(this ServerPlayer player, DialogData dialogData)
    {
        player.EmitLocked("dialog:create", dialogData);
    }

    public static void UpdateClothes(this ServerPlayer player)
    {
        player.EmitLocked("character:updateclothes", player.CharacterModel.InventoryModel);
    }

    public static void CreateTimer(this ServerPlayer player, string id, ElapsedEventHandler callback, int milliseconds,
        bool restart = false)
    {
        var timer = new Timer { Interval = milliseconds, AutoReset = false, Enabled = true };
        if (player.Timers.TryAdd(id, timer))
        {
            timer.Elapsed += callback;
            timer.AutoReset = restart;
            timer.Start();
        }
    }

    public static void ClearTimer(this ServerPlayer player, string id)
    {
        if (player.Timers.TryRemove(id, out var timer))
        {
            timer.Dispose();
        }
    }

    public static void ClearAllTimer(this ServerPlayer player)
    {
        foreach (var key in player.Timers.Keys)
        {
            if (player.Timers.TryRemove(key, out var timer))
            {
                timer.Dispose();
            }
        }
    }
}