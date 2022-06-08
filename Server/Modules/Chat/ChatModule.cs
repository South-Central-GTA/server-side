using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using Microsoft.Extensions.Logging;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.CustomLogs;
using Server.Helper;

namespace Server.Modules.Chat;

public class ChatModule
    : ITransientScript
{
    private readonly ChatLogService _chatLogService;
    private readonly ILogger<ChatModule> _logger;
    private readonly Serializer _serializer;

    public ChatModule(
        ILogger<ChatModule> logger,
        Serializer serializer,
        ChatLogService chatLogService)
    {
        _logger = logger;
        _serializer = serializer;
        _chatLogService = chatLogService;
    }

    public async Task SendProxMessage(ServerPlayer player, float radius, ChatType type, string context)
    {
        var afterName = GetChatAfterName(type);
        var beforeChat = GetBeforeChat(player, type);

        var name = player.IsAduty ? player.AccountName : player.CharacterModel.Name;
        var afterChat = GetAfterChat(name, type);

        SendMessage(player.Position,
                    player.Dimension,
                    radius,
                    new ChatMessageData
                    {
                        Sender = type != ChatType.DO ? name : null,
                        Context = context,
                        AfterName = afterName,
                        BeforeChat = beforeChat,
                        AfterChat = afterChat,
                        SendetAt = _serializer.Serialize(DateTime.Now),
                        ChatType = type
                    });

        await _chatLogService.Add(new ChatLogModel
        {
            AccountModelId = player.AccountModel.SocialClubId,
            CharacterModelId = player.CharacterModel.Id,
            ChatType = type,
            Text = context,
            LoggedAt = DateTime.Now
        });
    }

    public void SendProxMessage(string name, float radius, ChatType type, string context, Position position,
                                int dimension)
    {
        var afterName = GetChatAfterName(type);
        var beforeChat = GetBeforeChat(type);
        var afterChat = GetAfterChat(name, type);

        SendMessage(position,
                    dimension,
                    radius,
                    new ChatMessageData
                    {
                        Sender = type != ChatType.DO ? name : null,
                        Context = context,
                        AfterName = afterName,
                        BeforeChat = beforeChat,
                        AfterChat = afterChat,
                        SendetAt = _serializer.Serialize(DateTime.Now),
                        ChatType = type
                    });
    }

    public void SendMessage(ServerPlayer player, string? name, ChatType type, string message, string color)
    {
        var chatMessage = new ChatMessageData
        {
            Sender = type != ChatType.DO ? name : null,
            Context = message,
            Color = color,
            AfterName = GetChatAfterName(type),
            BeforeChat = GetBeforeChat(player, type),
            AfterChat = GetAfterChat(name, type),
            SendetAt = _serializer.Serialize(DateTime.Now),
            ChatType = type
        };

        player.EmitLocked("chat:pushmessage", player.Dimension, chatMessage);
    }

    private void SendMessage(Position position, int dimension, float radius, ChatMessageData chatMessage)
    {
        var colorPattern = GetColorPattern(chatMessage.ChatType);

        foreach (var target in GetAllPlayer(position, radius, dimension))
        {
            chatMessage.Color = GetChatColor(radius, position, colorPattern, target);
            target.EmitLocked("chat:pushmessage", dimension, chatMessage, position);
        }
    }

    private string[] GetColorPattern(ChatType chatType)
    {
        var retVal = chatType switch
        {
            ChatType.EMOTE => new[] { "#C2A2DA", "#ae91c4", "#9b81ae", "#9b81ae", "#9b81ae" },
            ChatType.MY => new[] { "#C2A2DA", "#ae91c4", "#9b81ae", "#9b81ae", "#9b81ae" },
            ChatType.DO => new[] { "#C2A2DA", "#ae91c4", "#9b81ae", "#9b81ae", "#9b81ae" },
            ChatType.MEGAPHONE => new[] { "#f7de12", "#f7e129", "#f8e441", "#f9e759", "#faeb70" },
            _ => new[] { "#E6E6E6", "#C8C8C8", "#AAAAAA", "#8C8C8C", "#6E6E6E" }
        };

        return retVal;
    }

    private string GetChatAfterName(ChatType chatType)
    {
        string retVal = null;

        switch (chatType)
        {
            case ChatType.EMOTE:
                retVal = " ";
                break;

            case ChatType.MY:
                retVal = "'s ";
                break;

            case ChatType.DO:
                retVal = "";
                break;

            case ChatType.MEGAPHONE:
                retVal = ": [Megafon]: ";
                break;

            case ChatType.OOC:
            case ChatType.ADMIN_CHAT:
                retVal = ": ";
                break;

            case ChatType.PHONE_SPEAK:
            case ChatType.RADIO_SPEAK:
            case ChatType.DEP_SPEAK:
            case ChatType.SPEAK:
                retVal = " sagt: ";
                break;

            case ChatType.PHONE_WISPER:
            case ChatType.RADIO_WISPER:
            case ChatType.DEP_WISPER:
            case ChatType.WISPER:
                retVal = " flüstert: ";
                break;

            case ChatType.PHONE_SCREAM:
            case ChatType.RADIO_SCREAM:
            case ChatType.DEP_SCREAM:
            case ChatType.SCREAM:
                retVal = " schreit: ";
                break;
        }

        return retVal;
    }

    private string GetBeforeChat(ServerPlayer player, ChatType chatType)
    {
        string retVal = null;
        var freq = 0;

        if (player.GetData("RADIO_FREQUENCY", out int playerFreq))
        {
            freq = playerFreq;
        }

        switch (chatType)
        {
            case ChatType.EMOTE:
            case ChatType.MY:
            case ChatType.DO:
                retVal = "* ";
                break;

            case ChatType.OOC:
                retVal = "(( ";
                break;

            case ChatType.ADMIN_CHAT:
                retVal = "[A] ";
                break;

            case ChatType.RADIO_SPEAK:
            case ChatType.RADIO_SCREAM:
            case ChatType.RADIO_WISPER:
                retVal = $"**[RADIO FQ: {freq}] ";
                break;

            case ChatType.DEP_SPEAK:
            case ChatType.DEP_SCREAM:
            case ChatType.DEP_WISPER:
                retVal = "**[RADIO FQ: 99] ";
                break;
        }

        return retVal;
    }

    private string GetBeforeChat(ChatType chatType)
    {
        string retVal = null;

        switch (chatType)
        {
            case ChatType.EMOTE:
            case ChatType.MY:
            case ChatType.DO:
                retVal = "* ";
                break;

            case ChatType.OOC:
                retVal = "(( ";
                break;
        }

        return retVal;
    }

    private string GetAfterChat(string? name, ChatType chatType)
    {
        string retVal = null;

        switch (chatType)
        {
            case ChatType.DO:
                retVal = $" (( {name} ))";
                break;

            case ChatType.WISPER:
                retVal = "";
                break;

            case ChatType.OOC:
                retVal = " ))";
                break;

            case ChatType.MEGAPHONE:
                break;
        }

        return retVal;
    }

    private IEnumerable<ServerPlayer> GetAllPlayer(Position position, float radius, int dimension)
    {
        var players = Alt.GetAllPlayers()
                         .GetByRange(position, radius)
                         .FindAll(p => p.Dimension == dimension);

        var bigEarsPlayers = Alt.GetAllPlayers().Where(p => p.IsInBigEars);
        players.AddRange(bigEarsPlayers);

        return players;
    }

    private string GetChatColor(float radius, Position position, string[] colorPattern, ServerPlayer target)
    {
        var color = colorPattern[0];

        if (target.IsInBigEars)
        {
            return color;
        }

        if (position.Distance(target.Position) > radius / 4)
        {
            color = colorPattern[1];
        }

        if (position.Distance(target.Position) > radius / 3)
        {
            color = colorPattern[2];
        }

        if (position.Distance(target.Position) > radius / 2)
        {
            color = colorPattern[3];
        }

        if (position.Distance(target.Position) > radius)
        {
            color = colorPattern[4];
        }

        return color;
    }
}