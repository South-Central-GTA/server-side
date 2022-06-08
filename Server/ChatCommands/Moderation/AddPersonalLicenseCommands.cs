using System;
using AltV.Net;
using Server.Core.CommandSystem;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Character;
using Server.Core.Abstractions.ScriptStrategy;

namespace Server.ChatCommands.Moderation;

public class AddPersonalLicenseCommands : ISingletonScript
{
    private readonly PersonalLicenseService _personalLicenseService;

    public AddPersonalLicenseCommands(PersonalLicenseService personalLicenseService)
    {
        _personalLicenseService = personalLicenseService;
    }

    [Command("addclicense",
             "Gebe einem Charakter eine bestimmte Lizenz.",
             Permission.STAFF,
             new[] { "Spieler ID", "Lizenz" })]
    public async void OnExecute(ServerPlayer player, string expectedPlayerId, string expectedLicense)
    {
        if (!player.Exists)
        {
            return;
        }

        var target = Alt.GetAllPlayers().GetPlayerById(player, expectedPlayerId);
        if (target == null)
        {
            return;
        }

        if (!Enum.TryParse(expectedLicense, out PersonalLicensesType personalLicensesType))
        {
            player.SendNotification("Gebe eine korrekte persönliche Lizenz an.", NotificationType.ERROR);
            return;
        }

        if (await _personalLicenseService.Has(l => l.Type == personalLicensesType
                                                   && l.CharacterModelId == target.CharacterModel.Id))
        {
            player.SendNotification("Der Charakter besitzt diese Lizenz schon.", NotificationType.ERROR);
            return;
        }

        await _personalLicenseService.Add(new PersonalLicenseModel()
        {
            CharacterModelId = target.CharacterModel.Id,
            Type = personalLicensesType
        });

        player.CharacterModel.Licenses =
            await _personalLicenseService.Where(l => l.CharacterModelId == player.CharacterModel.Id);

        player.SendNotification("Du hast dem Charakter " + target.CharacterModel.Name + " die Lizenz " +
                                personalLicensesType +
                                " hinzugefügt.",
                                NotificationType.SUCCESS);
        target.SendNotification("Du hast von " + player.AccountName + " die Lizenz " + personalLicensesType +
                                " administrativ hinzugefügt bekommen.",
                                NotificationType.SUCCESS);
    }
}