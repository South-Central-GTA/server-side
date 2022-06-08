using System.Linq;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Database.Models.Inventory;
using Server.Modules.Inventory;
using Server.Modules.Weapon;

namespace Server.Handlers.Inventory;

public class WeaponAttachmentItemHandler : ISingletonScript
{
    private readonly AttachmentModule _attachmentModule;

    private readonly InventoryModule _inventoryModule;
    private readonly ItemService _itemService;

    public WeaponAttachmentItemHandler(
        ItemService itemService,
        AttachmentModule attachmentModule,
        InventoryModule inventoryModule)
    {
        _itemService = itemService;

        _attachmentModule = attachmentModule;
        _inventoryModule = inventoryModule;

        AltAsync.OnClient<ServerPlayer, int>("item:removeattachment", OnPlayerRemoveAttachment);
    }

    private async void OnPlayerRemoveAttachment(ServerPlayer player, int attachedAttachmentId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (player.CharacterModel.InventoryModel.Items.FirstOrDefault(i => i.Id == attachedAttachmentId) is not
            ItemWeaponAttachmentModel weaponAttachment)
        {
            return;
        }

        await _attachmentModule.RemoveFromWeapon(player, weaponAttachment.ItemModelWeapon, weaponAttachment);

        await _inventoryModule.UpdateInventoryUiAsync(player);
    }
}