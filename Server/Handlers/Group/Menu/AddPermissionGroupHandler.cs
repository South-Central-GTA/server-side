﻿using System;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.Group;

namespace Server.Handlers.Group;

public class AddPermissionGroupHandler : ISingletonScript
{
    private readonly GroupModule _groupModule;
    private readonly GroupService _groupService;

    public AddPermissionGroupHandler(GroupService groupService, GroupModule groupModule)
    {
        _groupService = groupService;
        _groupModule = groupModule;

        AltAsync.OnClient<ServerPlayer, int, int, string>("groupmenu:addpermission", OnAddPermission);
    }

    private async void OnAddPermission(ServerPlayer player, int groupId, int level, string expectedGroupPermission)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!Enum.TryParse(expectedGroupPermission, true, out GroupPermission groupPermission))
        {
            return;
        }

        var group = await _groupService.GetByKey(groupId);
        if (group == null)
        {
            return;
        }

        if (!_groupModule.IsOwner(player, group))
        {
            player.SendNotification("Dein Charakter muss Eigentümer von der Gruppe sein:", NotificationType.ERROR);
            return;
        }

        var success = await _groupModule.AddRankPermission(groupId, level, groupPermission);
        if (success)
        {
            await _groupModule.UpdateGroupUi(group);
        }
    }
}