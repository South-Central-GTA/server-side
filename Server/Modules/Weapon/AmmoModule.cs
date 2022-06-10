using AltV.Net.Async;
using Microsoft.Extensions.Logging;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Database.Enums;

namespace Server.Modules.Weapon;

public class AmmoModule : ITransientScript
{
    private readonly ILogger<AmmoModule> _logger;
    private readonly WeaponModule _weaponModule;

    public AmmoModule(ILogger<AmmoModule> logger, WeaponModule weaponModule)
    {
        _logger = logger;
        _weaponModule = weaponModule;
    }

    public void Give(ServerPlayer player, ItemCatalogIds ammoItemCatalogId, int amount)
    {
        player.EmitLocked("weapon:giveammo", ammoItemCatalogId.ToString(), amount);
    }

    public void Remove(ServerPlayer player, ItemCatalogIds ammoItemCatalogId, int amount)
    {
        player.EmitLocked("weapon:removeammo", ammoItemCatalogId.ToString(), amount);
    }

    public static bool IsItemAmmo(ItemCatalogIds itemCatalogId)
    {
        switch (itemCatalogId)
        {
            case ItemCatalogIds.AMMO_PISTOL:
            case ItemCatalogIds.AMMO_MACHINE_GUN:
            case ItemCatalogIds.AMMO_ASSAULT:
            case ItemCatalogIds.AMMO_SNIPER:
            case ItemCatalogIds.AMMO_SHOTGUN:
            case ItemCatalogIds.AMMO_LIGHT_MACHINE_GUN:
                return true;
            default:
                return false;
        }
    }
}