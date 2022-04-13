using System;
using System.Linq;
using AltV.Net;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.CommandSystem;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;

namespace Server.ChatCommands.Moderation;

public class RemovePersonalLicenseCommands : ISingletonScript
{
    private readonly PersonalLicenseService _personalLicenseService;

    public RemovePersonalLicenseCommands(PersonalLicenseService personalLicenseService)
    {
        _personalLicenseService = personalLicenseService;
    }

    [Command("removeclicense", "Entferne einem Charakter eine bestimmte Lizenz.", Permission.STAFF, new[] { "Spieler ID", "Lizenz" })]
    public async void OnRemoveCharacterLicense(ServerPlayer player, string expectedPlayerId, string expectedLicense)
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

        if (!Enum.TryParse(expectedLicense, out PersonalLicensesType characterLicense))
        {
            player.SendNotification("Gebe eine korrekte persönliche Lizenz an.", NotificationType.ERROR);
            return;
        }

        var personalLicenseModel = target.CharacterModel.Licenses.FirstOrDefault(l => l.Type == characterLicense);

        if (personalLicenseModel == null)
        {
            player.SendNotification("Es wurde keine Lizenz gefunden.", NotificationType.ERROR);
            return;
        }
        
        await _personalLicenseService.Remove(personalLicenseModel);

        player.CharacterModel.Licenses = await _personalLicenseService.Where(l => l.CharacterModelId == player.CharacterModel.Id);

        player.SendNotification("Du hast dem Charakter " + target.CharacterModel.Name + " die Lizenz " + characterLicense + " entfernt.", NotificationType.SUCCESS);
        target.SendNotification("Du hast von " + player.AccountName + " die Lizenz " + characterLicense + " administrativ entfernt bekommen.", NotificationType.SUCCESS);
    }
}