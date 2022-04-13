using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using AltV.Net.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Server.Core.Abstractions;
using Server.Core.Configuration;
using Server.Data.Enums.EntitySync;
using Server.DataAccessLayer.Services;
using Server.Helper;
using Server.Modules;
using Server.Modules.EntitySync;

namespace Server.ServerJobs;

public class RoleplayInfos : IServerJob
{
    private readonly DevelopmentOptions _developmentOptions;
    private readonly GameOptions _gameOptions;
    private readonly ILogger<DroppedItems> _logger;

    private readonly MarkerSyncModule _markerSyncModule;

    private readonly RoleplayInfoService _roleplayInfoService;
    private readonly Serializer _serializer;

    public RoleplayInfos(
        ILogger<DroppedItems> logger,
        IOptions<GameOptions> gameOptions,
        IOptions<DevelopmentOptions> developmentOptions,
        Serializer serializer,
        RoleplayInfoService roleplayInfoService,
        MarkerSyncModule markerSyncModule)
    {
        _logger = logger;
        _gameOptions = gameOptions.Value;
        _developmentOptions = developmentOptions.Value;
        _serializer = serializer;

        _roleplayInfoService = roleplayInfoService;

        _markerSyncModule = markerSyncModule;
    }

    public async Task OnSave()
    {
        await Task.CompletedTask;
    }

    public async Task OnShutdown()
    {
        await Task.CompletedTask;
    }

    public async Task OnStartup()
    {
        if (_developmentOptions.DropDatabaseAtStartup)
        {
            return;
        }

        var infos = await _roleplayInfoService.GetAll();

        var infosToDelete = infos.Where(i => (i.CreatedAt - DateTime.Now).TotalDays >=
                                             _gameOptions.DeleteRoleplayInfosAfterDays);

        infos.RemoveAll(i => (i.CreatedAt - DateTime.Now).TotalDays >=
                             _gameOptions.DeleteRoleplayInfosAfterDays);

        await _roleplayInfoService.RemoveRange(infosToDelete);

        foreach (var info in infos)
        {
            var marker = _markerSyncModule.Create(MarkerType.QUESTION_MARK,
                                                  info.Position,
                                                  Vector3.Zero,
                                                  Vector3.Zero,
                                                  new Vector3(0.5f),
                                                  new Rgba(245, 230, 83, 70),
                                                  info.Dimension,
                                                  false,
                                                  200,
                                                  "",
                                                  info.CharacterModel.Name,
                                                  _serializer.Serialize(info.CreatedAt));

            info.MarkerId = marker.Id;
        }

        await _roleplayInfoService.UpdateRange(infos);

        await Task.CompletedTask;
    }
}