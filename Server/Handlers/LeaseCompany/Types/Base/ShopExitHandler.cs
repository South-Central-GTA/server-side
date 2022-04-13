using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Housing;
using Server.Modules.Chat;
using Server.Modules.Narrator;

namespace Server.Handlers.LeaseCompany.Types.Base;

public class ShopExitHandler : ISingletonScript
{
    private readonly GroupService _groupService;
    private readonly HouseService _houseService;
    private readonly ItemService _itemService;
    
    private readonly ChatModule _chatModule;
    private readonly NarratorModule _narratorModule;

    private readonly Random _rand = new();

    private readonly List<string> _sentencesVariations = new()
    {
        "Ey komm' zurück und bezahl die Scheiße die du eingesteckt hast!",
        "Wat soll der Scheiß denn jetzt, beweg dein Hintern wieder rein und zahl!",
        "Du hast was vergessen! Komm zurück und zahl deine Ware!",
        "Ich ruf die Bullen, komm sofort zurück und bezahl!",
        "Dumme Idee man! Ich ruf die Bullen, komm zurück und bezahl den Mist!",
        "Du hast vergessen zu bezahlen! Bewege dein Arsch zurück!"
    };

    private readonly UserShopDataService _userShopDataService;

    public ShopExitHandler(
        HouseService houseService,
        ItemService itemService,
        GroupService groupService,
        UserShopDataService userShopDataService,
        
        ChatModule chatModule,
        NarratorModule narratorModule)
    {
        _houseService = houseService;
        _itemService = itemService;
        _groupService = groupService;
        _userShopDataService = userShopDataService;

        _chatModule = chatModule;
        _narratorModule = narratorModule;

        AltAsync.OnClient<ServerPlayer, uint>("interior:enter", OnEnteredStore);
        AltAsync.OnClient<ServerPlayer, uint>("interior:left", OnLeaveStore);
    }

    private async void OnEnteredStore(ServerPlayer player, uint mloInterior)
    {
        if (!player.Exists)
        {
            return;
        }

        if (await _houseService.GetByDistance(player.Position, 20) is not LeaseCompanyHouseModel leaseCompanyHouse)
        {
            return;
        }

        if (!await GotWarned(player))
        {
            return;
        }

        if (!leaseCompanyHouse.CashierX.HasValue
            || !leaseCompanyHouse.CashierY.HasValue
            || !leaseCompanyHouse.CashierZ.HasValue
            || !leaseCompanyHouse.CashierHeading.HasValue)
        {
            return;
        }

        var genderString = player.CharacterModel.Gender == GenderType.MALE ? "er" : "sie";
        var cashierPos = new Position(leaseCompanyHouse.CashierX.Value, leaseCompanyHouse.CashierY.Value, leaseCompanyHouse.CashierZ.Value);

        _chatModule.SendProxMessage("Kassierer",
                                    20,
                                    ChatType.EMOTE,
                                    $"schaut zu {player.CharacterModel.Name} als {genderString} zurück in den Laden kommt.",
                                    cashierPos,
                                    0);

        _chatModule.SendProxMessage("Kassierer",
                                    20,
                                    ChatType.SPEAK,
                                    "Hast' ja nochmal Glück gehabt, gute Entscheidung. Bezahl jetzt die Scheiße und verschwinde.",
                                    cashierPos,
                                    0);

        player.ClearTimer("shop_rob");
    }

    private async void OnLeaveStore(ServerPlayer player, uint mloInterior)
    {
        if (!player.Exists)
        {
            return;
        }

        if (await _houseService.GetByDistance(player.Position, 10) is not LeaseCompanyHouseModel leaseCompanyHouse)
        {
            return;
        }

        var shopData = await _userShopDataService.GetByKey(player.CharacterModel.Id);
        if (shopData == null)
        {
            return;
        }

        if (leaseCompanyHouse.PlayerDuty)
        {
            await RemovePlayerFromData(player, true);

            var value = _rand.NextDouble();
            if (value <= 0.3) // 30 chance 
            {
                return;
            }

            var group = await _groupService.GetByKey(leaseCompanyHouse.GroupModelId);
            if (group == null)
            {
                return;
            }

            foreach (var groupMember in group.Members)
            {
                var target = Alt.GetAllPlayers().FindPlayerByCharacterId(groupMember.CharacterModelId);
                if (target is { IsDuty: true })
                {
                    _narratorModule.SendMessage(target,
                                                $"Deinem Charakter fällt auf, dass {player.CharacterModel.Name} gerade mit unbezahlten Waren rausgegangen ist.");
                }
            }
        }
        else
        {
            if (!leaseCompanyHouse.HasCashier
                || !leaseCompanyHouse.CashierX.HasValue
                || !leaseCompanyHouse.CashierY.HasValue
                || !leaseCompanyHouse.CashierZ.HasValue
                || !leaseCompanyHouse.CashierHeading.HasValue)
            {
                await RemovePlayerFromData(player, true);
                return;
            }

            var genderString = player.CharacterModel.Gender == GenderType.MALE ? "ihm" : "ihr";
            var cashierPos = new Position(leaseCompanyHouse.CashierX.Value, leaseCompanyHouse.CashierY.Value, leaseCompanyHouse.CashierZ.Value);

            if (await GotWarned(player))
            {
                _chatModule.SendProxMessage("Kassierer",
                                            20,
                                            ChatType.EMOTE,
                                            $"schaut kurz zu {player.CharacterModel.Name} schüttelt den Kopf und drückt eine Taste auf der Kasse.",
                                            cashierPos,
                                            0);

                _chatModule.SendProxMessage("Kassierer",
                                            3,
                                            ChatType.DO,
                                            "Die Taste ist Gelb und ein kleines schwarzes Telefon ist darauf platziert.",
                                            cashierPos,
                                            0);

                _chatModule.SendProxMessage("Kassierer",
                                            3,
                                            ChatType.SPEAK,
                                            "Ich lass mich doch nicht verarschen...",
                                            cashierPos,
                                            0);


                await CallPolice(player);
            }
            else
            {
                _chatModule.SendProxMessage("Kassierer",
                                            20,
                                            ChatType.EMOTE,
                                            $"schaut zu {player.CharacterModel.Name} und ruft {genderString} nach.",
                                            cashierPos,
                                            0);

                _chatModule.SendProxMessage("Kassierer",
                                            20,
                                            ChatType.SPEAK,
                                            _sentencesVariations[_rand.Next(_sentencesVariations.Count)],
                                            cashierPos,
                                            0);

                await SetWarned(player);
            }
        }
    }

    private async Task RemovePlayerFromData(ServerPlayer player, bool stoleItems)
    {
        player.CharacterModel.InventoryModel.Items.ForEach(i =>
        {
            if (!i.IsBought)
            {
                i.IsBought = true;
                i.IsStolen = stoleItems;
            }
        });

        await _itemService.UpdateRange(player.CharacterModel.InventoryModel.Items);

        var shopData = await _userShopDataService.GetByKey(player.CharacterModel.Id);
        if (shopData != null)
        {
            await _userShopDataService.Remove(shopData);
        }
    }

    private async Task SetWarned(ServerPlayer player)
    {
        var shopData = await _userShopDataService.GetByKey(player.CharacterModel.Id);
        if (shopData == null)
        {
            return;
        }

        shopData.GotWarned = true;
        await _userShopDataService.Update(shopData);

        player.CreateTimer("shop_rob", (sender, args) => OnShopRobTimerCallback(player), _rand.Next(8000, 12000));
    }

    private async void OnShopRobTimerCallback(ServerPlayer player)
    {
        await CallPolice(player);
    }

    private async Task<bool> GotWarned(ServerPlayer player)
    {
        var shopData = await _userShopDataService.GetByKey(player.CharacterModel.Id);
        return shopData is { GotWarned: true };
    }

    private async Task CallPolice(ServerPlayer player)
    {
        if (_rand.NextDouble() <= 0.5) // 50/50 chance 
        {
            if (!player.Exists)
            {
                return;
            }

            //TODO: Add actual call to police here over mdc.

            var costs = await GetBill(player);

            player.SendNotification($"Debug: Information ans PD, Diebstahl in Wert von {costs}$.", NotificationType.WARNING);
        }

        await RemovePlayerFromData(player, true);
    }

    private async Task<int> GetBill(ServerPlayer player)
    {
        var shopData = await _userShopDataService.GetByKey(player.CharacterModel.Id);
        return shopData?.BillToPay ?? 0;
    }
}