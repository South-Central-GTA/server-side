using System;
using System.Numerics;
using System.Threading.Tasks;
using AltV.Net.Data;
using Microsoft.Extensions.Logging;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Enums.EntitySync;
using Server.DataAccessLayer.Services;
using Server.Database.Models;
using Server.Helper;
using Server.Modules.EntitySync;

namespace Server.Modules.RoleplayInfo;

public class RoleplayInfoModule : ISingletonScript
{
    private readonly ILogger<RoleplayInfoModule> _logger;

    private readonly MarkerSyncModule _markerSyncModule;

    private readonly RoleplayInfoService _roleplayInfoService;
    private readonly Serializer _serializer;

    public RoleplayInfoModule(ILogger<RoleplayInfoModule> logger, Serializer serializer,
        RoleplayInfoService roleplayInfoService, MarkerSyncModule markerSyncModule)
    {
        _logger = logger;
        _serializer = serializer;

        _roleplayInfoService = roleplayInfoService;

        _markerSyncModule = markerSyncModule;
    }

    public async Task<bool> ReachedLimit(ServerPlayer player)
    {
        var infos = await _roleplayInfoService.Where(i => i.CharacterModelId == player.CharacterModel.Id);
        return infos.Count >= player.AccountModel.MaxRoleplayInfos;
    }

    public async Task AddInfo(ServerPlayer player, int distance, string expectedInfo)
    {
        var marker = _markerSyncModule.Create(MarkerType.QUESTION_MARK, player.Position, Vector3.Zero, Vector3.Zero,
            new Vector3(0.5f), new Rgba(245, 230, 83, 70), player.Dimension, false, 200, "", player.CharacterModel.Name,
            _serializer.Serialize(DateTime.Now));

        await _roleplayInfoService.Add(new RoleplayInfoModel
        {
            MarkerId = marker.Id,
            CharacterModelId = player.CharacterModel.Id,
            Dimension = player.Dimension,
            Distance = distance,
            Context = expectedInfo,
            PositionX = player.Position.X,
            PositionY = player.Position.Y,
            PositionZ = player.Position.Z,
            Roll = player.Rotation.Roll,
            Pitch = player.Rotation.Pitch,
            Yaw = player.Rotation.Yaw
        });

        player.SendNotification("Du hast eine Info erstellt.", NotificationType.SUCCESS);
    }

    public async Task DeleteInfo(ServerPlayer player)
    {
        var closestInfo = await _roleplayInfoService.GetByDistance(player.Position);
        if (closestInfo == null)
        {
            player.SendNotification("Es ist keine Information in deiner Nähe.", NotificationType.ERROR);
            return;
        }

        await _roleplayInfoService.Remove(closestInfo);
        _markerSyncModule.Delete(closestInfo.MarkerId);

        player.SendNotification("Du hast eine Info entfernt.", NotificationType.SUCCESS);
    }
}