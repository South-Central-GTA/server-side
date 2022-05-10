using System.Linq;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.EmergencyCall;

namespace Server.Modules.Phone;

public class PhoneCallModule : ISingletonScript
{
    private readonly ItemPhoneService _itemPhoneService;
    private readonly EmergencyCallDialogModule _emergencyCallDialogModule;
    
    public PhoneCallModule(
        ItemPhoneService itemPhoneService, 
        EmergencyCallDialogModule emergencyCallDialogModule)
    {
        _itemPhoneService = itemPhoneService;
        _emergencyCallDialogModule = emergencyCallDialogModule;
    }
    
    /// Lost connection boolean is true for example when the player dies.
    public void Hangup(ServerPlayer player, bool lostConnection = false)
    {
        if (!player.Exists)
        {
            return;
        }

        if (player.PhoneCallData == null)
        {
            return;
        }

        if (lostConnection)
        {
            player.SendNotification("Die Verbindung wurde unterbrochen.", NotificationType.INFO);
            player.EmitLocked("phone:close");
        }
        else
        {
            player.SendNotification("Du hast aufgelegt.", NotificationType.SUCCESS);
        }

        if (player.PhoneCallData != null)
        {
            if (player.PhoneCallData.PartnerPhoneNumber == "911")
            {
                _emergencyCallDialogModule.Stop(player);
                return;
            }
            
            var caller = Alt.GetAllPlayers().GetPlayerById(player, player.PhoneCallData.PartnerPlayerId, false);
            if (caller is { Exists: true })
            {
                caller.EmitLocked("phone:callgothungup");
                caller.SendNotification(!lostConnection ? "Es wurde aufgelegt." : "Die Verbindung wurde unterbrochen.",
                                        NotificationType.INFO);

                caller.PhoneCallData = null;
            }
        }
    
        player.PhoneCallData = null;
    }

    public async Task CallPhoneAsync(ServerPlayer player, string numberToCall, string callerNumber)
    {
        if (!player.Exists)
        {
            return;
        }

        if (numberToCall == callerNumber)
        {
            player.SendNotification("Dein Charakter kann sich nicht selbst anrufen.", NotificationType.ERROR);
            return;
        }

        if (player.PhoneCallData != null)
        {
            player.SendNotification("Dein Charakter befindet sich schon in einem Telefonat.", NotificationType.ERROR);
            return;
        }

        var callerPhone = await _itemPhoneService.Find(p => p.PhoneNumber == callerNumber);
        if (callerPhone == null)
        {
            return;
        }

        var phoneToCall = await _itemPhoneService.Find(p => p.PhoneNumber == numberToCall);
        if (phoneToCall == null)
        {
            return;
        }

        await AltAsync.Do(() =>
        {
            if (phoneToCall.CurrentOwnerId == null)
            {
                player.EmitGui("phone:connectionfailed");
                return;
            }

            var playerToCall = Alt.GetAllPlayers().FindPlayerByCharacterId(phoneToCall.CurrentOwnerId.Value);

            // TODO: Add logic if the called player not online. 
            // Add something to the call history and cancel the callers call after 20 seconds.

            if (playerToCall == null)
            {
                player.SendNotification("Spieler ist offline.", NotificationType.ERROR);
                return;
            }

            if (playerToCall.PhoneCallData != null)
            {
                player.EmitGui("phone:numberisbusy");
                return;
            }

            var contact = phoneToCall.Contacts.Find(c => c.PhoneNumber == callerNumber);
            playerToCall.EmitLocked("phone:getcallfrom",
                                    contact != null ? contact.Name : callerNumber,
                                    phoneToCall.Id);

            var phoneItemToCall = playerToCall.CharacterModel.InventoryModel.Items
                                              .FirstOrDefault(i => i.Id == phoneToCall.Id
                                                                   && i.CatalogItemModelId == ItemCatalogIds.PHONE);

            var phoneNote = phoneItemToCall?.Note != null ? $"({phoneItemToCall.Note})" : "";
            playerToCall.SendNotification($"Das Handy {phoneNote} klingelt.",
                                          NotificationType.INFO);

            var callerContact = callerPhone.Contacts
                                           .Find(c => c.PhoneNumber == numberToCall);

            player.EmitLocked("phone:callnumber", callerContact != null ? callerContact.Name : numberToCall);

            player.PhoneCallData = new PhoneCallData(playerToCall.Id, numberToCall, true);
            playerToCall.PhoneCallData = new PhoneCallData(player.Id, callerPhone.PhoneNumber, false);
        });
    }

    public void DenyCall(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        AltAsync.Do(() =>
        {
            if (player.PhoneCallData == null)
            {
                return;
            }

            var caller = Alt.GetAllPlayers().GetPlayerById(player, player.PhoneCallData.PartnerPlayerId, false);
            if (caller == null)
            {
                return;
            }

            caller.EmitGui("phone:callgotdenied");

            caller.PhoneCallData = null;
            player.PhoneCallData = null;
        });
    }

    public async Task AcceptCallAsync(ServerPlayer player, int phoneId)
    {
        if (!player.Exists)
        {
            return;
        }

        var playerPhone = await _itemPhoneService.GetByKey(phoneId);
        if (playerPhone == null)
        {
            return;
        }
        
        await AltAsync.Do(() =>
        {
            if (player.PhoneCallData == null)
            {
                return;
            }

            var caller = Alt.GetAllPlayers().GetPlayerById(player, player.PhoneCallData.PartnerPlayerId);
            if (caller is not { Exists: true })
            {
                return;
            }

            if (playerPhone.CurrentOwnerId != player.CharacterModel.Id)
            {
                return;
            }

            var contact = playerPhone.Contacts.Find(c => c.PhoneNumber == player.PhoneCallData.PartnerPhoneNumber);

            caller.EmitGui("phone:connectcall");
            player.EmitGui("phone:openactivecall", contact != null ? contact.Name : player.PhoneCallData.PartnerPhoneNumber);
        });
    }
}