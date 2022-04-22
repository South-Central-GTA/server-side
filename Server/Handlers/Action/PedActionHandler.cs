using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Housing;
using Server.Modules.Context;

namespace Server.Handlers.Action;

public class PedActionHandler : ISingletonScript
{
    private readonly HouseService _houseService;
    
    private readonly ContextModule _contextModule;
    
    private readonly uint[] _publicGaragePeds = { Alt.Hash("mp_m_waremech_01"), Alt.Hash("mp_f_bennymech_01"), Alt.Hash("cs_jimmyboston") };
    private readonly uint[] _drivingSchoolPeds = { Alt.Hash("u_m_y_baygor"), Alt.Hash("u_m_y_burgerdrug_01") };
    private readonly uint _cityHallPed = Alt.Hash("u_m_y_gunvend_01");
    public PedActionHandler(
        HouseService houseService, 
        
        ContextModule contextModule)
    {
        _houseService = houseService;
        
        _contextModule = contextModule;
        AltAsync.OnClient<ServerPlayer, uint, int>("pedactions:get", OnGetActions);
    }

    private async void OnGetActions(ServerPlayer player, uint entityModel, int playerId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (playerId != -1)
        {
            var targetPlayer = Alt.GetAllPlayers().FindPlayerById(playerId);
            if (targetPlayer != null)
            {
                await HandlePlayerPed(player, targetPlayer);
                return;
            }
        }

        if (entityModel == Alt.Hash("mp_m_shopkeep_01")) // Player interacts with the supermarket npc.
        {
            _contextModule.OpenMenu(player, "Kassierer", new List<ActionData>()
            {
                new($"Waren bezahlen", "supermarket:requestopencashiermenu"),
                new($"Waren zurückgeben", "supermarket:requestreturnitems")
            });
        }
        else if (entityModel == Alt.Hash("ig_zimbor")) // Player interacts with the clothing store npc.
        {
            _contextModule.OpenMenu(player, "Kassierer", new List<ActionData>()
            {
                new($"Umkleide benutzen", "clothingstore:requeststartchangeclothes"),
                new($"Waren bezahlen", "clothingstore:requestopencashiermenu"),
                new($"Waren zurückgeben", "clothingstore:requestreturnitems")
            });
        }
        else if (entityModel == Alt.Hash("s_f_y_shop_low")) // Player interacts with the hair studio npc.
        {
            _contextModule.OpenMenu(player, "Friseurin", new List<ActionData>()
            {
                new($"Haare schneiden", "hairsalon:requeststarthaircut"),
            });
        }
        else if (entityModel == Alt.Hash("u_m_y_tattoo_01")) // Player interacts with the tatto studio npc.
        {
            _contextModule.OpenMenu(player, "Friseurin", new List<ActionData>()
            {
                new($"Tattoos stechen", "tattoostudio:requeststarttattoos"),
            });
        }
        else if (entityModel == Alt.Hash("s_m_y_ammucity_01")) // Player interacts with the ammunation npc.
        {
            _contextModule.OpenMenu(player, "Friseurin", new List<ActionData>()
            {
                new($"Waren einkaufen", "ammunation:requestopenmenu"),
            });
        }
        else if (_publicGaragePeds.Contains(entityModel))
        {
            _contextModule.OpenMenu(player, "Public Garage Mitarbeiter", new List<ActionData>()
            {
                new("Fahrzeug einparken", "publicgarage:requestparkvehicle"),
                new("Fahrzeug ausparken", "publicgarage:requestparkedvehicles"),
                new("Fahrzeug respawnen", "publicgarage:requestdestroyedvehicles"),
            });
        }
        else if (_drivingSchoolPeds.Contains(entityModel))
        {
            _contextModule.OpenMenu(player, "Public Garage Mitarbeiter", new List<ActionData>()
            {
                new("Führerschein Prüfung beginnen", "drivingschool:showstartdialog"),
            });
        }
        else if (_cityHallPed == entityModel)
        {
            _contextModule.OpenMenu(player, "LS Stadthalle", new List<ActionData>()
            {
                new("Meldeamt", "cityhall:requestmenu"),
            });
        }
    }

    private async Task HandlePlayerPed(ServerPlayer player, ServerPlayer targetPlayer)
    {
        var actions = new List<ActionData>()
        {
            new($"{targetPlayer.CharacterModel.Name} durchsuchen", "frisk:requestsearch", targetPlayer.Id)
        };

        if (targetPlayer.IsDuty)
        {
            var house = await _houseService.GetByKey(targetPlayer.DutyLeaseCompanyHouseId);
            if (house is LeaseCompanyHouseModel companyHouseModel)
            {
                switch (companyHouseModel.LeaseCompanyType)
                {
                    case LeaseCompanyType.SUPERMARKET:
                        actions.AddRange(new List<ActionData>()
                        {
                            new("Waren bezahlen", "supermarket:requestopencashiermenu"),
                            new("Waren zurückgeben", "supermarket:requestreturnitems")
                        });
                        break;
                    case LeaseCompanyType.CLOTHING_STORE:
                        actions.AddRange(new List<ActionData>()
                        {
                            new("Umkleide benutzen", "clothingstore:requeststartchangeclothes"),
                            new("Waren bezahlen", "clothingstore:requestopencashiermenu"),
                            new("Waren zurückgeben", "clothingstore:returnitems")
                        });
                        break;
                    case LeaseCompanyType.HAIR_STUDIO:
                        actions.Add(new("Haare schneiden", "hairsalon:starthaircut"));
                        break;
                    case LeaseCompanyType.TATTOO_STUDIO:
                        actions.Add(new("Tattos stechen", "tattoostudio:starttattoos"));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        _contextModule.OpenMenu(player, targetPlayer.CharacterModel.Name, actions);
    }
}