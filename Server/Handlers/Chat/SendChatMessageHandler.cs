using AltV.Net;
using AltV.Net.Async;
using Microsoft.Extensions.Logging;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Database.Enums;
using Server.Modules.Chat;
using Server.Modules.EmergencyCall;

namespace Server.Handlers.Chat;

public class SendChatMessageHandler : ISingletonScript
{
    private readonly ChatModule _chatModule;
    private readonly EmergencyCallDialogModule _emergencyCallDialogModule;
    private readonly ILogger<SendChatMessageHandler> _logger;

    public SendChatMessageHandler(ILogger<SendChatMessageHandler> logger, ChatModule chatModule,
        EmergencyCallDialogModule emergencyCallDialogModule)
    {
        _logger = logger;
        _chatModule = chatModule;
        _emergencyCallDialogModule = emergencyCallDialogModule;

        AltAsync.OnClient<ServerPlayer, string>("chat:sendmessage", OnPlayerSendChatMessage);
    }

    private async void OnPlayerSendChatMessage(ServerPlayer player, string message)
    {
        if (!player.Exists)
        {
            return;
        }

        message = message.TrimEnd().TrimStart();

        if (string.IsNullOrEmpty(message))
        {
            player.SendNotification("Man kann keine leeren Nachrichten verschicken.", NotificationType.ERROR);
            return;
        }

        if (player.CharacterModel.DeathState == DeathState.DEAD)
        {
            player.SendNotification("Dein Charakter kann jetzt nicht reden.", NotificationType.ERROR);
            return;
        }

        if (player.PhoneCallData != null)
        {
            await _chatModule.SendProxMessage(player, 15, ChatType.PHONE_SPEAK, message);

            if (player.PhoneCallData.PartnerPhoneNumber == "911")
            {
                await _emergencyCallDialogModule.SendDialogAnswer(player, message);
                return;
            }

            var callPartnerPlayer =
                Alt.GetAllPlayers().GetPlayerById(player, player.PhoneCallData.PartnerPlayerId, false);
            if (callPartnerPlayer == null)
            {
                return;
            }

            _chatModule.SendMessage(callPartnerPlayer, player.CharacterModel.Name, ChatType.PHONE_SPEAK, message,
                "#f3f59f");
            return;
        }

        // Just send a normal chat message if it no command.
        await _chatModule.SendProxMessage(player, 15, ChatType.SPEAK, message);
    }
}