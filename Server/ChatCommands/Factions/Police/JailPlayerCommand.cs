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

internal class JailPlayerCommand : ISingletonScript
{
    private readonly WorldLocationOptions _worldLocationOptions;
    private readonly GroupFactionService _groupFactionService;
    private readonly CharacterService _characterService;

    private readonly PrisonModule _prisonModule;

    public JailPlayerCommand(
        IOptions<WorldLocationOptions> worldLocationOptions,
        GroupFactionService groupFactionService,
        CharacterService characterService,
        PrisonModule prisonModule)
    {
        _worldLocationOptions = worldLocationOptions.Value;
        _groupFactionService = groupFactionService;
        _characterService = characterService;

        _prisonModule = prisonModule;
    }

    [Command("jail",
             "Packe einen Charakter ins Gefängnis.",
             Permission.NONE,
             new[] { "Spieler ID", "Dauer in Minuten" })]
    public async void OnExecute(ServerPlayer player, string expectedPlayerId, string expectedDuration)
    {
        if (!player.Exists)
        {
            return;
        }

        var factionGroup = await _groupFactionService.GetFactionByCharacter(player.CharacterModel.Id);
        if (factionGroup is not { FactionType: FactionType.POLICE_DEPARTMENT })
        {
            player.SendNotification("Das kann dein Charakter nicht.", NotificationType.ERROR);
            return;
        }

        var target = Alt.GetAllPlayers().GetPlayerById(player, expectedPlayerId);
        if (target == null)
        {
            return;
        }

        if (!int.TryParse(expectedDuration, out var duration))
        {
            player.SendNotification("Bitte gebe ein richtige Zahl für die Dauer in Minuten an.",
                                    NotificationType.ERROR);
            return;
        }

        if (player.Position.Distance(target.Position) > 3.0f)
        {
            player.SendNotification("Dein Charakter ist zu weit entfernt.", NotificationType.ERROR);
            return;
        }

        if (player.Position.Distance(new Position(_worldLocationOptions.JailPositionX,
                                                  _worldLocationOptions.JailPositionY,
                                                  _worldLocationOptions.JailPositionZ)) > 5.0f)
        {
            player.SendNotification("Dein Charakter muss hinter dem Police Department in Davis sein.",
                                    NotificationType.ERROR);
            return;
        }

        target.CharacterModel.JailedUntil = DateTime.Now.AddMinutes(duration);
        target.CharacterModel.JailedByCharacterName = player.CharacterModel.Name;

        await _characterService.Update(target.CharacterModel);

        _prisonModule.SetPlayerInPrison(target);

        player.SendNotification($"Dein Charakter hat {target.CharacterModel.Name} ins Gefängnis gesperrt.",
                                NotificationType.SUCCESS);
    }
}