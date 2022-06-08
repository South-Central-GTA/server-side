using AltV.Net.Async;
using Microsoft.Extensions.Logging;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;

namespace Server.Handlers.Chat;

public class SetTypingHandler : ISingletonScript
{
    private readonly ILogger<SetTypingHandler> _logger;

    public SetTypingHandler(
        ILogger<SetTypingHandler> logger)
    {
        _logger = logger;

        AltAsync.OnClient<ServerPlayer, bool>("chat:settyping", OnPlayerSetTyping);
    }

    private async void OnPlayerSetTyping(ServerPlayer player, bool state)
    {
        if (!player.Exists)
        {
            return;
        }

        _logger.LogInformation($"Player: {player.Name} typing state changed to: {state}.");
        await player.SetSyncedMetaDataAsync("IS_TYPING", state);
    }
}