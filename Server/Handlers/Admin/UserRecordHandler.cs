using System;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.CustomLogs;

namespace Server.Handlers.Admin;

public class UserRecordHandler : ISingletonScript
{
    private readonly UserRecordLogService _userRecordLogService;
    
    public UserRecordHandler(UserRecordLogService userRecordLogService)
    {
        _userRecordLogService = userRecordLogService;
        
        AltAsync.OnClient<ServerPlayer, ulong>("userrecord:request", OnRequestUserRecord);
        AltAsync.OnClient<ServerPlayer, ulong, string>("userrecord:saveentry", OnRequestSaveUserRecordEntry);
    }

    private async void OnRequestUserRecord(ServerPlayer player, ulong accountId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.AccountModel.Permission.HasFlag(Permission.STAFF))
        {
            return;
        }

        player.EmitGui("userrecord:setup", accountId, await _userRecordLogService.Where(ur => ur.AccountModelId == accountId));
    }

    private async void OnRequestSaveUserRecordEntry(ServerPlayer player, ulong accountId, string manuelEntry)
    {
        await _userRecordLogService.Add(new UserRecordLogModel
        {
            AccountModelId = accountId,
            StaffAccountModelId = player.AccountModel.SocialClubId,
            CharacterModelId = player.CharacterModel.Id,
            UserRecordType = UserRecordType.BY_HUMAN,
            Text = manuelEntry,
            LoggedAt = DateTime.Now
        });

        player.EmitGui("userrecord:setup", accountId, await _userRecordLogService.Where(ur => ur.AccountModelId == accountId));
        player.SendNotification("Eintrag wurde erfolgreich gespeichert.", NotificationType.SUCCESS);
    }
}