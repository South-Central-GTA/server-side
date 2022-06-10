using System.Threading.Tasks;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Configuration;
using Server.Core.Entities;
using Server.Core.Extensions;

namespace Server.Handlers.Session;

public class PlayerConnectHandler : ISingletonScript
{
    private readonly DevelopmentOptions _devOptions;
    private readonly ILogger<PlayerDisconnectHandler> _logger;
    private readonly WorldData _worldData;
    private readonly WorldLocationOptions _worldLocationOptions;

    public PlayerConnectHandler(ILogger<PlayerDisconnectHandler> logger, WorldData worldData,
        IOptions<WorldLocationOptions> worldLocationOptions, IOptions<DevelopmentOptions> devOptions)
    {
        _devOptions = devOptions.Value;
        _logger = logger;
        _worldData = worldData;
        _worldLocationOptions = worldLocationOptions.Value;
        AltAsync.OnPlayerConnect += (player, reason) => OnPlayerConnect((ServerPlayer)player, reason);
    }

    private async Task OnPlayerConnect(ServerPlayer player, string reason)
    {
        if (!player.Exists)
        {
            return;
        }

        var uiUrl = "http://resource/gui/index.html";
        if (_devOptions.DebugUi)
        {
            uiUrl = _devOptions.CefIp4;
        }

        _logger.LogInformation("Requesting UI from {url}", uiUrl);
        player.EmitLocked("webview:create", uiUrl);

        _logger.LogInformation("Connection: SID {socialClub} with IP {ip}", player.SocialClubId, player.Ip);

        player.SetUniqueDimension();
        player.SetPosition(_worldLocationOptions.LoginPositionX, _worldLocationOptions.LoginPositionY,
            _worldLocationOptions.LoginPositionZ);

        player.SetDateTime(_worldData.Clock.Day, _worldData.Clock.Month, _worldData.Clock.Year, _worldData.Clock.Hour,
            _worldData.Clock.Minute, _worldData.Clock.Second);
        player.ClearData();
    }
}