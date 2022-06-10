using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Helper;
using Server.Modules.Inventory;

namespace Server.Handlers.Weapon;

public class WeaponHandler : ISingletonScript
{
    private readonly InventoryModule _inventoryModule;
    private readonly Serializer _serializer;

    public WeaponHandler(Serializer serializer, InventoryModule inventoryModule)
    {
        _serializer = serializer;
        _inventoryModule = inventoryModule;

        AltAsync.OnClient<ServerPlayer, string>("weapon:sendammo", OnSendAmmo);
    }

    private async void OnSendAmmo(ServerPlayer player, string ammoJson)
    {
        await _inventoryModule.UpdateAmmo(player, ammoJson);
    }
}