using System.Collections.Generic;
using System.Threading.Tasks;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Models.Inventory;
using Server.Helper;
using Server.Modules.Clothing;
using Server.Modules.Context;
using Server.Modules.EntitySync;

namespace Server.Handlers.Action;

public class ObjectActionHandler : ISingletonScript
{
    private readonly Serializer _serializer;
    private readonly ItemService _itemService;
    
    private readonly ObjectSyncModule _objectSyncModule;
    private readonly ContextModule _contextModule;
    
    public ObjectActionHandler(
        Serializer serializer, 
        ItemService itemService,
        
        ObjectSyncModule objectSyncModule, 
        ContextModule contextModule)
    {
        _serializer = serializer;
        _itemService = itemService;
        
        _objectSyncModule = objectSyncModule;
        _contextModule = contextModule;

        AltAsync.OnClient<ServerPlayer, uint, uint, ulong?>("objectactions:get", OnGetActions);
    }

    private async void OnGetActions(ServerPlayer player, uint entityId, uint model, ulong? objId)
    {
        if (!player.Exists)
        {
            return;
        }
        
        var actions = new List<ActionData>();
        
        switch (model) {
            case 2930269768: // walledin atms
            case 3168729781: 
            case 506770882: 
                actions.Add(new ActionData("Menu öffnen", "atm:requestopenmenu"));
                break;
            case 3424098598: 
                actions.Add(new ActionData("Menu öffnen", "atm:requestopenmenu"));
                break;
            case 756199591: 
            case 2067313593: 
            case 1437777724: 
            case 1421582485: 
            case 4240248142: 
            case 3762902115: 
            case 643522702: 
            case 4074731919: 
            case 1793329478: 
            case 3352088677: 
            case 4241316616: 
            case 3492728915: 
            case 1987036371: 
                actions.Add(new ActionData("Einkaufen", "supermarket:requestopenmenu"));
                break;
            case 1339433404: 
            case 1933174915:
            case 2287735495:
            case 3825272565:
                actions.Add(new ActionData("Treibstoff bezahlen", "gasstation:requestopenmenu"));
                break;
            case 1245865676: // traffic cone
            case 289396019: // moneybag
            case 974883178: // phone
            case 977923025: // keys
            case 746336278: // non alc drink
            case 774094055: // alc drink
            case 936464539: // fast food
            case 910205311: // fruits
            case 1485704474: // bread
            case 3602873787: // sandwich
            case 3889866470: // soup
            case 1778631864: // meat and sweets
            case 3310697493: // candy
            case 3963794318: // handcuff keys
            case 2330564864: // radio
            case 2709416104: // megaphone
            case 1070220657: // handcuffs
            case 3369309184: // licenses
            case 887694239: // toolbox
            case 1725061196: // weapon - dagger
            case 1742452667: // weapon - baseball bat
            case 3505843344: // weapon - broken bottle
            case 495450405: // weapon - crowbar
            case 4228001377: // weapon - flashlight
            case 1933637837: // weapon - golfclub
            case 4167227990: // weapon - hammer
            case 173095431: // weapon - hatchet /stone hatchet
            case 1430410579: // weapon - small weapons (prop_box_guncase_01a) 
            case 3776622480: // weapon - knife / switchblade
            case 2239480765: // weapon - machete
            case 10555072: // weapon - wrench
            case 1184113278: // weapon - poolcue
            case 956623953: // weapon - long weapons (ch_prop_ch_guncase_01a)
            case 4258032409: // weapon - med size weps (prop_idol_case_02)
            case 2574153389: // weapon - ball
            case 2358755187: // weapon - bz gas
            case 445804908: // weapon - flare
            case 3628385663: // weapon - fire extin
            case 786272259: // weapon - jerry can
            case 1269906701: // weapon - parachute
            case 1580014892: // ammo - pistol
            case 669213687: // ammo - smg
            case 190687980: // ammo - rifle
            case 1093460780: // ammo - sniper
            case 1560006187: // ammo - shotgun                
            case 1843823183: // ammo - heavy                
            case 1470358132: // all attachments (prop_champ_box_01)    
            case 3575239779: // clothing box
                if (!objId.HasValue)
                {
                    return;
                }
                actions.AddRange(await HandlePickupAbleItem(player, objId.Value));
                break;
            default:
                player.SendNotification("Keine Interaktionen mit diesem Objekt möglich.", NotificationType.ERROR);
                break;
        }

        _contextModule.OpenMenu(player, "Interaktion", actions);
    }

    private async Task<List<ActionData>> HandlePickupAbleItem(ServerPlayer player, ulong objId)
    {
        var serverObject = _objectSyncModule.Get(objId);
        if (serverObject == null)
        {
            return new List<ActionData>();
        }

        var item = await _itemService.GetByKey(serverObject.ItemId);
        if (item == null)
        {
            return new List<ActionData>();
        }

        var actions = new List<ActionData>()
        {
            new($"{GetItemName(item)} aufheben", "placeableitem:pickup", serverObject.Id)
        };

        if (ClothingModule.IsClothesOrProp(item.CatalogItemModelId))
        {
            actions.Add(new ActionData($"{GetItemName(item)} anziehen", "placeableitem:puton", serverObject.Id));
        }

        if (player.IsAduty)
        {
            actions.Add(new ActionData($"[Admin] {GetItemName(item)} löschen", "placeableitem:deleteitem", serverObject.Id));
        }
        
        return actions;
    }
    
    private string GetItemName(ItemModel itemModel)
    {
        if (ClothingModule.IsClothesOrProp(itemModel.CatalogItemModelId))
        {
            if (string.IsNullOrEmpty(itemModel.CustomData))
            {
                return "";
            }
            
            var data = _serializer.Deserialize<ClothingData>(itemModel.CustomData);
            return data.Title;
        }

        return itemModel.CatalogItemModel.Name;
    }
}