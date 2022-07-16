using System;
using AltV.Net;
using AltV.Net.Data;
using Microsoft.Extensions.Options;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.CommandSystem;
using Server.Core.Configuration;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.Prison;

namespace Server.ChatCommands.Factions.Police;

internal class UnJailPlayerCommand : ISingletonScript
{
    private readonly CharacterService _characterService;
    private readonly FactionGroupService _factionGroupService;

    private readonly PrisonModule _prisonModule;
    private readonly WorldLocationOptions _worldLocationOptions;

    public UnJailPlayerCommand(IOptions<WorldLocationOptions> worldLocationOptions,
        FactionGroupService factionGroupService, CharacterService characterService, PrisonModule prisonModule)
    {
        _worldLocationOptions = worldLocationOptions.Value;
        _factionGroupService = factionGroupService;
        _characterService = characterService;

        _prisonModule = prisonModule;
    }

    [Command("unjail", "Entlasse einen Charakter aus dem Gefängnis.", Permission.NONE, new[] { "Charakter Name" },
        CommandArgs.GREEDY)]
    public async void OnExecute(ServerPlayer player, string expectedCharacterName)
    {
        if (!player.Exists)
        {
            return;
        }

        var factionGroup = await _factionGroupService.GetByCharacter(player.CharacterModel.Id);
        if (factionGroup is not { FactionType: FactionType.POLICE_DEPARTMENT })
        {
            player.SendNotification("Das kann dein Charakter nicht.", NotificationType.ERROR);
            return;
        }

        if (player.Position.Distance(new Position(_worldLocationOptions.JailPositionX,
                _worldLocationOptions.JailPositionY, _worldLocationOptions.JailPositionZ)) > 5.0f)
        {
            player.SendNotification("Dein Charakter muss hinter dem Police Department in Davis sein.",
                NotificationType.ERROR);
            return;
        }

        var targetCharacter =
            await _characterService.Find(c => c.FirstName + " " + c.LastName == expectedCharacterName);

        if (targetCharacter == null)
        {
            player.SendNotification("Es wurde kein Charakter mit diesem Namen gefunden.", NotificationType.ERROR);
            return;
        }

        if (!targetCharacter.JailedUntil.HasValue)
        {
            player.SendNotification("Der Charakter befindet sich nicht im Gefängnis.", NotificationType.ERROR);
            return;
        }

        var target = Alt.GetAllPlayers().FindPlayerByCharacterId(targetCharacter.Id);
        if (target != null)
        {
            await _prisonModule.ClearPlayerFromPrison(target);
            targetCharacter.JailedUntil = null;
        }
        else
        {
            targetCharacter.JailedUntil = DateTime.Now;
        }

        await _characterService.Update(targetCharacter);

        player.SendNotification($"Dein Charakter hat {targetCharacter.Name} aus dem Gefängnis entlassen.",
            NotificationType.SUCCESS);
    }
}