using System;
using System.Collections.Generic;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Modules.Narrator;
using Server.Database.Enums;

namespace Server.Handlers.Prison;

public class PrisonInmateCheckHandler : ISingletonScript
{
    private readonly GroupFactionService _groupFactionService;
    private readonly CharacterService _characterService;
    
    public PrisonInmateCheckHandler(
        GroupFactionService groupFactionService,
        CharacterService characterService)
    {
        _groupFactionService = groupFactionService;
        _characterService = characterService;
     
        AltAsync.OnClient<ServerPlayer>("prison:requestinmates", OnExecute);
    }

    private async void OnExecute(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        var factionGroup = await _groupFactionService.GetFactionByCharacter(player.CharacterModel.Id);
        if (factionGroup is not { FactionType: FactionType.POLICE_DEPARTMENT })
        {
            player.SendNotification("Dies würde dein Charakter nicht tun.", NotificationType.ERROR);
            return;
        }

        var prisonCharacters = await _characterService.Where(c => c.JailedUntil.HasValue);
        if (prisonCharacters.Count == 0)
        {
            player.SendNotification("Es sind keine Charaktere im Gefängnis.", NotificationType.INFO);
            return;
        }
        
        var characterList = new List<string>();
        prisonCharacters.ForEach(model =>
        {
            var span = model.JailedUntil.Value - DateTime.Now;
            characterList.Add($"<li>Name: {model.Name}, Haftzeit: {(int)span.TotalMinutes} Minuten, Eingesperrt von: {model.JailedByCharacterName}</li>");
        });
        
        player.CreateDialog(new DialogData
        {
            Type = DialogType.ONE_BUTTON_DIALOG,
            Title = "Gefangenen Übersicht",
            Description = $"<ul>{string.Join("", characterList)}<ul>",
            FreezeGameControls = true,
            PrimaryButton = "Okay",
        });
    }
}