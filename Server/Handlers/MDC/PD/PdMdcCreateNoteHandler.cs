﻿using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Mdc;
using Server.Modules.MDC;

namespace Server.Handlers.MDC.PD;

public class PdMdcCreateNoteHandler : ISingletonScript
{
    private readonly FactionGroupService _factionGroupService;
    private readonly MdcNoteService _mdcNoteService;

    private readonly PoliceMdcModule _policeMdcModule;

    public PdMdcCreateNoteHandler(FactionGroupService factionGroupService, MdcNoteService mdcNoteService,
        PoliceMdcModule policeMdcModule)
    {
        _policeMdcModule = policeMdcModule;
        _mdcNoteService = mdcNoteService;
        _factionGroupService = factionGroupService;

        AltAsync.OnClient<ServerPlayer, string, MdcSearchType, string>("policemdc:createnote", OnCreateNote);
    }

    private async void OnCreateNote(ServerPlayer player, string targetDbId, MdcSearchType mdcSearchType, string input)
    {
        if (!player.Exists)
        {
            return;
        }

        var factionGroup = await _factionGroupService.GetByCharacter(player.CharacterModel.Id);
        if (factionGroup is not { FactionType: FactionType.POLICE_DEPARTMENT })
        {
            return;
        }

        await _mdcNoteService.Add(new MdcNoteModel
        {
            TargetModelId = targetDbId,
            Type = mdcSearchType,
            CreatorCharacterName = player.CharacterModel.Name,
            Note = input
        });


        await _policeMdcModule.UpdateCurrentRecord(player, mdcSearchType, targetDbId);
    }
}