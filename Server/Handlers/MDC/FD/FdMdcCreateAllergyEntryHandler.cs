﻿using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.MDC;

namespace Server.Handlers.MDC.FD;

public class FdMdcCreateAllergyEntryHandler : ISingletonScript
{
    private readonly AllergiesModule _allergiesModule;
    private readonly FireMdcModule _fireMdcModule;
    private readonly FactionGroupService _factionGroupService;

    public FdMdcCreateAllergyEntryHandler(FactionGroupService factionGroupService, AllergiesModule allergiesModule,
        FireMdcModule fireMdcModule)
    {
        _factionGroupService = factionGroupService;

        _allergiesModule = allergiesModule;
        _fireMdcModule = fireMdcModule;

        AltAsync.OnClient<ServerPlayer, int, string>("firemdc:createallergy", OnCreateAllergyEntry);
    }

    private async void OnCreateAllergyEntry(ServerPlayer player, int characterId, string input)
    {
        if (!player.Exists)
        {
            return;
        }

        var factionGroup = await _factionGroupService.GetByCharacter(player.CharacterModel.Id);
        if (factionGroup is not { FactionType: FactionType.FIRE_DEPARTMENT })
        {
            return;
        }

        await _allergiesModule.Add(characterId, player.CharacterModel.Name, input);

        await _fireMdcModule.OpenPatientRecords(player, characterId);
    }
}