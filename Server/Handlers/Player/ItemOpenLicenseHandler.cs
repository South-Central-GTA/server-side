using System.Collections.Generic;
using System.Linq;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.Narrator;

namespace Server.Handlers.Player;

public class ItemOpenLicenseHandler : ISingletonScript
{
    private readonly CharacterService _characterService;
    private readonly ItemService _itemService;

    private readonly NarratorModule _narratorModule;
    
    public ItemOpenLicenseHandler(
        CharacterService characterService,
        ItemService itemService, 
        
        NarratorModule narratorModule)
    {
        _characterService = characterService;
        _itemService = itemService;
        
        _narratorModule = narratorModule;

        AltAsync.OnClient<ServerPlayer, int>("licenses:requestopen", OnLicensesRequestOpen);
    }

    private async void OnLicensesRequestOpen(ServerPlayer player, int itemId)
    {
        if (!player.Exists)
        {
            return;
        }

        var licItem = await _itemService.GetByKey(itemId);
        if (licItem?.CustomData == null)
        {
            return;
        }
        
        if (player.CharacterModel.InventoryModel.Id != licItem.InventoryModelId)
        {
            return;
        }

        var character = await _characterService.GetByKey(int.Parse(licItem.CustomData));
        if (character == null)
        {
            return;
        }
        
        if (character.Licenses.Count == 0)
        {
            _narratorModule.SendMessage(player, $"{character.Name} hat keine eingetragenden Lizenzen.");
            return;
        }

        var licenseStrings = new List<string>();
        foreach (var characterLicense in character.Licenses)
        {
            switch (characterLicense.Type)
            {
                case PersonalLicensesType.DRIVING:
                    licenseStrings.Add("Führerschein");
                    break;
                case PersonalLicensesType.BOATS:
                    licenseStrings.Add("Bootsschein");
                    break;
                case PersonalLicensesType.FLYING:
                    licenseStrings.Add("Flugschein");
                    break;
                case PersonalLicensesType.WEAPON:
                    licenseStrings.Add("Waffenschein");
                    break;
            }
        }

        _narratorModule.SendMessage(player, $"{character.Name} besitzt folgende Lizenzen: {string.Join(", ", licenseStrings)}.");
    }
}