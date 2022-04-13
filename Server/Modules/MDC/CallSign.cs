using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AltV.Net;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Models.Character;

namespace Server.Modules.MDC;

public class CallSign
{
    public Dictionary<string, List<CharacterModel>> CallSigns { get; } = new();

    private readonly GroupFactionService _groupFactionService;
    
    public CallSign(GroupFactionService groupFactionService)
    {
        _groupFactionService = groupFactionService;
    }

    public async Task AddCallSign(ServerPlayer player, string callSign)
    {
        if (CallSigns.ContainsKey(callSign))
        {
            CallSigns[callSign].Add(player.CharacterModel);
        }
        else
        {
            CallSigns.Add(callSign, new List<CharacterModel>() { player.CharacterModel });
        }
        
        await UpdateUi(player);
    }

    public async Task RemoveCallSign(ServerPlayer player)
    {
        var key = string.Empty;
        
        foreach (var callSign in CallSigns)
        {
            foreach (var character in callSign.Value)
            {
                if (character.Id == player.CharacterModel.Id)
                {
                    key = callSign.Key;
                }
            }
        }

        if (string.IsNullOrEmpty(key))
        {
            return;
        }

        CallSigns.TryGetValue(key, out var characters);
        
        characters?.RemoveAll(c => c.Id == player.CharacterModel.Id);
        if (characters?.Count == 0)
        {
            CallSigns.Remove(key);
        }
        else
        {
            CallSigns[key] = characters;
        }        
        
        await UpdateUi(player);
    }

    public async Task DeleteCallSign(ServerPlayer player, string callSign)
    {
        CallSigns.Remove(callSign);
        await UpdateUi(player);
    }
    
    public List<CallSignData> GetCallSigns()
    {
        return CallSigns.Select(callSign => new CallSignData()
        {
            CallSign = callSign.Key,
            Names = string.Join(", ", callSign.Value.Select(c => c.Name))
        }).ToList();
    }

    public bool HasCallSign(CharacterModel character)
    {
        foreach (var characters in CallSigns.Values)
        {
            return characters.Any(c => c.Id == character.Id);
        }

        return false;
    }

    public async Task UpdateUi(ServerPlayer player)
    {
        var factionGroup = await _groupFactionService.GetFactionByCharacter(player.CharacterModel.Id);
        if (factionGroup == null)
        {
            return;
        }
        
        foreach (var target in factionGroup.Members
                                           .Select(groupMember => Alt.GetAllPlayers().FindPlayerByCharacterId(groupMember.CharacterModelId))
                                           .Where(serverPlayer => serverPlayer != null))
        {
            target.EmitGui("mdc:updatecallsigns",GetCallSigns(), HasCallSign(target.CharacterModel)); 
        }
    }
}