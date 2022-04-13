using System.Linq;
using System.Threading.Tasks;
using AltV.Net;
using Microsoft.Extensions.Logging;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Extensions;
using Server.DataAccessLayer.Services;
using Server.Database.Models.Inventory;
using Server.Helper;
using Server.Modules.Weapon;

namespace Server.Modules.Inventory;

public class ItemDestructionModule
    : ITransientScript
{
    private readonly ItemService _itemService;
    private readonly ILogger<ItemDropModule> _logger;
    private readonly Serializer _serializer;

    private readonly WeaponModule _weaponModule;

    public ItemDestructionModule(
        ILogger<ItemDropModule> logger,
        Serializer serializer,
        ItemService itemService,
        WeaponModule weaponModule)
    {
        _logger = logger;
        _serializer = serializer;

        _itemService = itemService;

        _weaponModule = weaponModule;
    }

    public async Task Destroy(int itemId)
    {
        var item = await _itemService.GetByKey(itemId);
        if (item == null)
        {
            return;
        }

        if (WeaponModule.IsItemWeapon(item.CatalogItemModelId))
        {
            var items = await _itemService.GetAll();
            var weaponAttachmentItems = items.Where(i =>
                                                        i is ItemWeaponAttachmentModel weaponAttachment && weaponAttachment.ItemWeaponId == item.Id);
            await _itemService.RemoveRange(weaponAttachmentItems);

            if (item.InventoryModelId.HasValue && item.InventoryModel.CharacterModelId.HasValue)
            {
                var ownedPlayer = Alt.GetAllPlayers().FindPlayerByCharacterId(item.InventoryModel.CharacterModel.Id);
                if (ownedPlayer is { Exists: true })
                {
                    _weaponModule.Remove(ownedPlayer, WeaponModule.GetModelFromId(item.CatalogItemModelId));
                }
            }
        }

        await _itemService.Remove(item);
    }
}