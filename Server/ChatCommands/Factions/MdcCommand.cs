using System.Linq;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.CommandSystem;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;

namespace Server.ChatCommands.Factions;

internal class MdcCommand : ISingletonScript
{
    private readonly GroupFactionService _groupFactionService;

    public MdcCommand(GroupFactionService groupFactionService)
    {
        _groupFactionService = groupFactionService;
    }

    [Command("mdc", "Öffne den Mobile Data Computer.")]
    public async void OnExecute(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.IsInVehicle)
        {
            player.SendNotification("Dein Charakter muss in einem Fahrzeug sitzen.", NotificationType.ERROR);
            return;
        }

        if (player.Vehicle is not ServerVehicle vehicle)
        {
            return;
        }

        if (vehicle.DbEntity == null)
        {
            return;
        }

        if (!vehicle.DbEntity.GroupModelOwnerId.HasValue)
        {
            player.SendNotification("Dieses Fahrzeug hat keinen Mobile Data Computer.", NotificationType.ERROR);
            return;
        }

        var vehicleFactionGroup = await _groupFactionService.GetByKey(vehicle.DbEntity.GroupModelOwnerId.Value);
        if (vehicleFactionGroup == null || vehicleFactionGroup.FactionType == FactionType.CITIZEN)
        {
            player.SendNotification("Dieses Fahrzeug hat keinen Mobile Data Computer.", NotificationType.ERROR);
            return;
        }

        var canLogin = false;
        string rankName = null;
        var factionType = FactionType.CITIZEN;

        var factionGroup = await _groupFactionService.GetFactionByCharacter(player.CharacterModel.Id);
        if (factionGroup is not null && factionGroup.FactionType == vehicleFactionGroup.FactionType)
        {
            var member = factionGroup.Members.FirstOrDefault(m => m.CharacterModelId == player.CharacterModel.Id);
            if (member != null)
            {
                var rank = factionGroup.Ranks.FirstOrDefault(r => r.Level == member.RankLevel);
                if (rank != null)
                {
                    rankName = rank.Name;
                    canLogin = true;
                    factionType = factionGroup.FactionType;
                }
            }
        }

        player.EmitLocked("mdc:open", (int)factionType, canLogin, player.CharacterModel.Name, rankName);
    }
}