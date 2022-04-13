using System.Threading.Tasks;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using Microsoft.Extensions.Logging;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.Chat;
using Server.Modules.EntitySync;

namespace Server.Handlers.ColShape;

public class ColShapeHandler : ISingletonScript
{
    private readonly ChatModule _chatModule;
    private readonly ILogger<ColShapeHandler> _logger;

    private readonly MarkerSyncModule _markerSyncModule;

    private readonly RoleplayInfoService _roleplayInfoService;

    public ColShapeHandler(
        ILogger<ColShapeHandler> logger,
        RoleplayInfoService roleplayInfoService,
        MarkerSyncModule markerSyncModule,
        ChatModule chatModule)
    {
        _logger = logger;

        _roleplayInfoService = roleplayInfoService;

        _markerSyncModule = markerSyncModule;
        _chatModule = chatModule;

        AltAsync.OnColShape += AltAsyncOnOnColShape;
    }

    private async Task AltAsyncOnOnColShape(IColShape colShape, IEntity entity, bool state)
    {
        if (state)
        {
            if (entity is ServerPlayer player)
            {
                var marker = _markerSyncModule.GetMarker(colShape);
                if (marker != null)
                {
                    var info = await _roleplayInfoService.Find(i => i.MarkerId == marker.Id);
                    if (info != null)
                    {
                        _chatModule.SendMessage(player, "", ChatType.INFO, info.Context, "#C2A2DA");
                    }
                }
            }
        }
    }
}