using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models._Base;
using Server.Database.Models.Housing;
using Server.Database.Models.Inventory;
using Server.Database.Models.Vehicles;

namespace Server.Modules.Key;

public class KeyModule : ITransientScript
{
    private readonly GroupService _groupService;

    public KeyModule(GroupService groupService)
    {
        _groupService = groupService;
    }

    public async Task<HasKeyErrorType> HasKey(ServerPlayer player, ILockableEntity lockableEntity)
    {
        var isGroupEntity = false;

        HouseModel houseModel = null;
        PlayerVehicleModel vehicleModel = null;
        DoorModel doorModel = null;
        var keys = new List<int>();

        switch (lockableEntity)
        {
            case HouseModel h:
                if (h.GroupModelId.HasValue)
                {
                    isGroupEntity = true;
                }

                houseModel = h;
                keys = houseModel.Keys;
                break;
            case DoorModel d:
                if (d.HouseModel.GroupModelId.HasValue)
                {
                    isGroupEntity = true;
                }

                doorModel = d;
                keys = doorModel.HouseModel.Keys;
                break;
            case PlayerVehicleModel v:
                if (v.GroupModelOwnerId.HasValue)
                {
                    isGroupEntity = true;
                }

                vehicleModel = v;
                keys = vehicleModel.Keys;
                break;
        }

        var canInteract = false;
        var isNotInGroup = false;

        if (isGroupEntity)
        {
            foreach (var i in player.CharacterModel.InventoryModel.Items
                         .Where(i => i.CatalogItemModelId == ItemCatalogIds.GROUP_KEY).Cast<ItemGroupKeyModel>())
            {
                if (!i.GroupModelId.HasValue)
                {
                    continue;
                }

                var group = await _groupService.GetByKey(i.GroupModelId);
                if (group?.Members == null || group.Members.Count == 0)
                {
                    continue;
                }

                if (group.Members.Any(m => m.CharacterModelId == player.CharacterModel.Id))
                {
                    // Group keys are a bit diffrent, there are not in the key list there are just checking the group owner id.
                    canInteract = houseModel != null && houseModel?.GroupModelId == i.GroupModelId ||
                                  vehicleModel != null && vehicleModel?.GroupModelOwnerId == i.GroupModelId ||
                                  doorModel != null && doorModel.HouseModel.GroupModelId == i.GroupModelId;

                    if (canInteract)
                    {
                        return HasKeyErrorType.HAS_GROUP_KEY;
                    }
                }
                else
                {
                    isNotInGroup = houseModel != null && houseModel?.GroupModelId == i.GroupModelId ||
                                   vehicleModel != null && vehicleModel?.GroupModelOwnerId == i.GroupModelId;
                }
            }

            return isNotInGroup ? HasKeyErrorType.HAS_WRONG_GROUP_KEY : HasKeyErrorType.HAS_NO_KEY;
        }

        if (keys.Count == 0)
        {
            return HasKeyErrorType.HAS_NO_KEY;
        }

        // Last check if we are maybe able to use a normal key.
        var keyItem = player.CharacterModel.InventoryModel.Items.Where(i => i.CatalogItemModelId == ItemCatalogIds.KEY)
            .FirstOrDefault(i => keys.Any(k => k == i.Id));

        return keyItem == null ? HasKeyErrorType.HAS_NO_KEY : HasKeyErrorType.HAS_KEY;
    }
}