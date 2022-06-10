using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;

namespace Server.Handlers.Action;

public class InVehicleActionHandler : ISingletonScript
{
    public InVehicleActionHandler()
    {
        AltAsync.OnClient<ServerPlayer>("invehicleactions:get", OnGetActions);
    }

    private async void OnGetActions(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.IsInVehicle)
        {
        }

        // Implement actions based on in vehicle interactions.
    }
}