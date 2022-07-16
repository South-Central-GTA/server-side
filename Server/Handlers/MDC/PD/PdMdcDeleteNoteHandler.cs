﻿using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.Group;
using Server.Modules.MDC;

namespace Server.Handlers.MDC.PD;

public class PdMdcDeleteNoteHandler : ISingletonScript
{
    private readonly FactionGroupService _factionGroupService;
    private readonly GroupModule _groupModule;
    private readonly MdcNoteService _mdcNoteService;

    private readonly PoliceMdcModule _policeMdcModule;

    public PdMdcDeleteNoteHandler(FactionGroupService factionGroupService, MdcNoteService mdcNoteService,
        PoliceMdcModule policeMdcModule, GroupModule groupModule)
    {
        _policeMdcModule = policeMdcModule;
        _groupModule = groupModule;
        _mdcNoteService = mdcNoteService;
        _factionGroupService = factionGroupService;

        AltAsync.OnClient<ServerPlayer, int>("policemdc:deletenote", OnDeleteNote);
    }

    private async void OnDeleteNote(ServerPlayer player, int id)
    {
        if (!player.Exists)
        {
            return;
        }

        var factionGroup = await _factionGroupService.GetByCharacter(player.CharacterModel.Id);
        if (factionGroup == null)
        {
            return;
        }

        if (!await _groupModule.HasPermission(player.CharacterModel.Id, factionGroup.Id, GroupPermission.MDC_OPERATOR))
        {
            return;
        }

        var note = await _mdcNoteService.GetByKey(id);
        if (note == null)
        {
            return;
        }

        await _mdcNoteService.Remove(note);

        await _policeMdcModule.UpdateCurrentRecord(player, note.Type, note.TargetModelId);
    }
}