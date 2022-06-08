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

public class ResetPersonalLicenseCommands : ISingletonScript
{
    private readonly PersonalLicenseService _personalLicenseService;

    public ResetPersonalLicenseCommands(PersonalLicenseService personalLicenseService)
    {
        _personalLicenseService = personalLicenseService;
    }

    [Command("resetclicense", "Entferne einem Charakter alle Lizenzen.", Permission.STAFF, new[] { "Spieler ID" })]
    public async void OnExecute(ServerPlayer player, string expectedPlayerId)
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

        var personalLicenseModels =
            target.CharacterModel.Licenses.Where(l => l.CharacterModelId == target.CharacterModel.Id);
        await _personalLicenseService.RemoveRange(personalLicenseModels);

        player.CharacterModel.Licenses =
            await _personalLicenseService.Where(l => l.CharacterModelId == player.CharacterModel.Id);

        player.SendNotification("Du hast dem Charakter " + target.CharacterModel.Name + " alle Lizenzen entfernt.",
                                NotificationType.SUCCESS);
        target.SendNotification("Du hast von " + player.AccountName + " alle Lizenzen administrativ entfernt bekommen.",
                                NotificationType.SUCCESS);
    }
}