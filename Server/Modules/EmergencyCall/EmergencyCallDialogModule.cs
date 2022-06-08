using System;
using System.Threading.Tasks;
using AltV.Net.Async;
using Microsoft.Extensions.Logging;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Mdc;
using Server.Modules.Chat;

namespace Server.Modules.EmergencyCall;

public class EmergencyCallDialogModule
    : ITransientScript
{
    private readonly ILogger<EmergencyCallDialogModule> _logger;
    private readonly EmergencyCallService _emergencyCallService;
    private readonly RadioModule _radioModule;

    public EmergencyCallDialogModule(
        ILogger<EmergencyCallDialogModule> logger,
        EmergencyCallService emergencyCallService,
        RadioModule radioModule)
    {
        _logger = logger;
        _emergencyCallService = emergencyCallService;
        _radioModule = radioModule;
    }

    public void Start(ServerPlayer player, string callerNumber)
    {
        player.PhoneCallData = new PhoneCallData(0, "911", true);

        player.EmitGui("phone:connectcall");
        player.EmitLocked("emergencycall:sendmessage", "Emergency Hotline 911, benötigen Sie das LSPD oder LSFD?");
        player.SetData("EMERGENCY_CALL_STATE", 1);
        player.SetData("EMERGENCY_CALL_NUMBER", callerNumber);
    }

    public async Task SendDialogAnswer(ServerPlayer player, string message)
    {
        if (player.GetData("EMERGENCY_CALL_STATE", out int state))
        {
            switch (state)
            {
                case 1:
                {
                    var factionType = FactionType.CITIZEN;

                    if (message.ToUpper().Contains("LSPD"))
                    {
                        factionType = FactionType.POLICE_DEPARTMENT;
                    }
                    else if (message.ToUpper().Contains("LSFD"))
                    {
                        factionType = FactionType.FIRE_DEPARTMENT;
                    }

                    if (factionType == FactionType.CITIZEN)
                    {
                        player.EmitLocked("emergencycall:sendmessage",
                                          "Entschuldigen Sie, benötigen Sie das LSPD oder LSFD?");
                        player.SetData("EMERGENCY_CALL_STATE", 1);
                        return;
                    }

                    player.SetData("EMERGENCY_CALL_STATE", 2);
                    player.SetData("EMERGENCY_FACTION", message);
                    player.EmitLocked("emergencycall:sendmessage", "Was genau ist passiert?");
                }
                    break;
                case 2:
                {
                    player.SetData("EMERGENCY_CALL_STATE", 3);
                    player.SetData("EMERGENCY_SITUATION", message);
                    player.EmitLocked("emergencycall:sendmessage", "Wo befinden Sie sich gerade?");
                }
                    break;
                case 3:
                {
                    player.GetData("EMERGENCY_CALL_NUMBER", out string phoneNumber);
                    player.GetData("EMERGENCY_FACTION", out string faction);
                    player.GetData("EMERGENCY_SITUATION", out string situation);

                    var factionType = FactionType.CITIZEN;

                    if (faction.ToUpper().Contains("LSPD"))
                    {
                        factionType = FactionType.POLICE_DEPARTMENT;
                    }
                    else if (faction.ToUpper().Contains("LSFD"))
                    {
                        factionType = FactionType.FIRE_DEPARTMENT;
                    }

                    await _emergencyCallService.Add(
                        new EmergencyCallModel(phoneNumber, factionType, situation, message));

                    await _radioModule.SendMessageOnFaction("Dispatch",
                                                            ChatType.RADIO_SPEAK,
                                                            factionType,
                                                            $"Neuer 911 Notruf, Situation {situation}, Location: {message}");

                    player.EmitLocked("emergencycall:sendmessage",
                                      "Ihr Notruf wurde aufgenommen und wird schnellstmöglich bearbeitet.");
                    player.EmitLocked("phone:callgothungup");
                    Stop(player);
                }
                    break;
            }
        }
    }

    public void Stop(ServerPlayer player)
    {
        player.PhoneCallData = null;
        player.DeleteData("EMERGENCY_CALL_NUMBER");
        player.DeleteData("EMERGENCY_SITUATION");
        player.DeleteData("EMERGENCY_FACTION");
        player.DeleteData("EMERGENCY_CALL_STATE");
    }
}