﻿using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.Narrator;

namespace Server.Handlers.Items;

public class ItemPoliceTicketHandler : ISingletonScript
{
    private readonly BankAccountService _bankAccountService;
    private readonly CharacterService _characterService;
    private readonly FactionGroupService _factionGroupService;

    private readonly NarratorModule _narratorModule;
    private readonly PoliceTicketService _policeTicketService;

    public ItemPoliceTicketHandler(PoliceTicketService policeTicketService, CharacterService characterService,
        FactionGroupService factionGroupService, BankAccountService bankAccountService, NarratorModule narratorModule)
    {
        _policeTicketService = policeTicketService;
        _characterService = characterService;
        _factionGroupService = factionGroupService;
        _bankAccountService = bankAccountService;

        _narratorModule = narratorModule;

        AltAsync.OnClient<ServerPlayer, int>("ticket:show", OnRequestShowTicket);
    }

    private async void OnRequestShowTicket(ServerPlayer player, int itemId)
    {
        var policeTicketItem = await _policeTicketService.GetByKey(itemId);
        if (policeTicketItem == null)
        {
            return;
        }

        var targetCharacter = await _characterService.GetByKey(policeTicketItem.TargetCharacterId);
        if (targetCharacter == null)
        {
            return;
        }

        var pdFaction = await _factionGroupService.Find(gf => gf.FactionType == FactionType.POLICE_DEPARTMENT);
        if (pdFaction == null)
        {
            return;
        }

        var pdBankAccount = await _bankAccountService.GetByGroup(pdFaction.Id);
        if (pdBankAccount == null)
        {
            return;
        }

        _narratorModule.SendMessage(player,
            $"Dein Charakter schaut sich einen Strafzettel für {targetCharacter.Name} an wo folgende Informationen draufstehen, Grund: {policeTicketItem.Reason}, Kosten {policeTicketItem.Costs}$, Referenznummer {policeTicketItem.ReferenceId}, Police Department Bankverbindung: {pdBankAccount.BankDetails} unterschrieben von {policeTicketItem.CreatorCharacterName}.");
    }
}