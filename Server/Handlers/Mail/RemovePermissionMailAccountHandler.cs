using System;
using AltV.Net;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.Mail;

namespace Server.Handlers.Mail;

public class RemovePermissionMailAccountHandler : ISingletonScript
{
    private readonly MailAccountService _mailAccountService;
    private readonly MailModule _mailModule;

    public RemovePermissionMailAccountHandler(
        MailAccountService mailAccountService,
        MailModule mailModule)
    {
        _mailAccountService = mailAccountService;

        _mailModule = mailModule;

        AltAsync.OnClient<ServerPlayer, string, int, string>("mailing:removepermission", OnRemovePermission);
    }

    private async void OnRemovePermission(ServerPlayer player, string mailAddress, int characterId, string expectedPermission)
    {
        if (!player.Exists)
        {
            return;
        }

        var mailAccount = await _mailAccountService.GetByKey(mailAddress);
        if (mailAccount == null)
        {
            player.SendNotification("Das Mailkonto existiert nicht mehr.", NotificationType.ERROR);
            return;
        }

        if (!await _mailModule.IsOwner(player, mailAccount))
        {
            return;
        }

        if (!Enum.TryParse(expectedPermission, true, out MailingPermission mailingPermission))
        {
            return;
        }

        var success = await _mailModule.RemovePermission(mailAccount, characterId, mailingPermission);
        if (!success)
        {
            return;
        }

        await _mailModule.UpdateUi(player);

        var targetPlayer = Alt.GetAllPlayers().FindPlayerByCharacterId(characterId);
        if (targetPlayer != null)
        {
            await _mailModule.UpdateUi(targetPlayer);
        }
    }
}