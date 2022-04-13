using System;
using System.Threading.Tasks;
using AltV.Net;
using Microsoft.Extensions.Logging;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Inventory;

namespace Server.Modules.Chat;

public class RadioModule
    : ITransientScript
{
    private readonly ChatModule _chatModule;
    private readonly ItemRadioService _itemRadioService;
    private readonly ILogger<RadioModule> _logger;

    public RadioModule(
        ILogger<RadioModule> logger,
        ItemRadioService itemRadioService,
        ChatModule chatModule)
    {
        _logger = logger;
        _itemRadioService = itemRadioService;

        _chatModule = chatModule;
    }

    public async Task SendMessageOnFrequency(ServerPlayer senderPlayer, ChatType chatType, ItemRadioModel radioModel, string message)
    {
        switch (radioModel.FactionType)
        {
            case FactionType.CITIZEN:
                await SendMessageOnCitizen(senderPlayer, chatType, radioModel, message);
                break;
            case FactionType.POLICE_DEPARTMENT:
            case FactionType.FIRE_DEPARTMENT:
                var displayName = senderPlayer.CharacterModel.FirstName[0] + ". " + senderPlayer.CharacterModel.LastName;
                await SendMessageOnFaction(displayName, chatType, radioModel.FactionType, message, radioModel.Id);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(radioModel.FactionType), radioModel.FactionType, null);
        }
    }

    public async Task SendMessageOnFaction(string senderName, ChatType chatType, FactionType factionType,
                                           string message, int radioModelId = -1)
    {
        var factionRadios = await _itemRadioService.Where(r => r.FactionType == factionType 
                                                               && r.Id != radioModelId);
        foreach (var otherRadio in factionRadios)
        {
            if (!otherRadio.InventoryModelId.HasValue)
            {
                continue;
            }

            if (!otherRadio.InventoryModel.CharacterModelId.HasValue)
            {
                continue;
            }

            var targetPlayer = Alt.GetAllPlayers().FindPlayerByCharacterId(otherRadio.InventoryModel.CharacterModelId.Value);
            if (targetPlayer != null)
            {
                _chatModule.SendMessage(targetPlayer, senderName, chatType, message, "#eceba9");
            }
        }
    }
    public async Task SendMessageOnDepartment(ServerPlayer senderPlayer, ChatType chatType, ItemRadioModel radioModel, string message)
    {
        var factionRadios = await _itemRadioService.Where(r => r.FactionType != FactionType.CITIZEN && r.Id != radioModel.Id);
        foreach (var otherRadio in factionRadios)
        {
            if (!otherRadio.InventoryModelId.HasValue)
            {
                continue;
            }

            if (!otherRadio.InventoryModel.CharacterModelId.HasValue)
            {
                continue;
            }

            var targetPlayer = Alt.GetAllPlayers().FindPlayerByCharacterId(otherRadio.InventoryModel.CharacterModelId.Value);
            var displayName = senderPlayer.CharacterModel.FirstName[0] + ". " + senderPlayer.CharacterModel.LastName;
            if (targetPlayer != null)
            {
                _chatModule.SendMessage(targetPlayer, displayName, chatType, message, "#d4d398");
            }
        }
    }
    
    private async Task SendMessageOnCitizen(ServerPlayer senderPlayer, ChatType chatType, ItemRadioModel radioModel,
                                            string message)
    {
        var radios = await _itemRadioService.Where(r => r.Frequency == radioModel.Frequency && r.Id != radioModel.Id);
        foreach (var otherRadio in radios)
        {
            if (!otherRadio.InventoryModelId.HasValue)
            {
                continue;
            }

            if (!otherRadio.InventoryModel.CharacterModelId.HasValue)
            {
                continue;
            }

            var targetPlayer = Alt.GetAllPlayers().FindPlayerByCharacterId(otherRadio.InventoryModel.CharacterModelId.Value);
            var displayName = senderPlayer.CharacterModel.FirstName[0] + ". " + senderPlayer.CharacterModel.LastName;
            if (targetPlayer != null)
            {
                _chatModule.SendMessage(targetPlayer, displayName, chatType, message, "#eceba9");
            }
        }
    }
}