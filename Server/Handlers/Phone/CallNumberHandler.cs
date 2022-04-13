using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Models;
using Server.Modules.EmergencyCall;
using Server.Modules.Phone;

namespace Server.Handlers.Phone;

public class CallNumberHandler : ISingletonScript
{
    private readonly PhoneCallModule _phoneCallModule;
    private readonly EmergencyCallDialogModule _emergencyCallDialogModule;

    public CallNumberHandler(
        PhoneCallModule phoneCallModule, 
        EmergencyCallDialogModule emergencyCallDialogModule)
    {
        _phoneCallModule = phoneCallModule;
        _emergencyCallDialogModule = emergencyCallDialogModule;
        
        AltAsync.OnClient<ServerPlayer, string, string>("phone:call", OnCallNumber);
    }

    private async void OnCallNumber(ServerPlayer player, string numberToCall, string callerNumber)
    {
        if (numberToCall == "911")
        {
            _emergencyCallDialogModule.Start(player, callerNumber);
            return;
        }
        
        await _phoneCallModule.CallPhoneAsync(player, numberToCall, callerNumber);
    }
}