using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Enums;
using Microsoft.Extensions.Logging;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Inventory;
using Server.Modules.Clothing;
using Server.Modules.Death;
using Server.Modules.Phone;
using Server.Modules.Weapon;

namespace Server.Modules.Character;

public class CharacterSpawnModule : ITransientScript
{
    private readonly AmmoModule _ammoModule;
    private readonly DeathModule _deathModule;
    private readonly FactionGroupService _factionGroupService;
    private readonly ClothingModule _clothingModule;

    private readonly ILogger<CharacterSpawnModule> _logger;
    private readonly PhoneModule _phoneModule;

    private readonly List<SpawnLocation> _spawns = new()
    {
        new SpawnLocation(0, "LS International Airport", new Position(-1037.011f, -2730.4746f, 13.744385f),
            new Rotation(0, 0, 0.7915824f),
            new List<LocationData>
            {
                new(new Position(-988.95825f, -2706.7253f, 13.32312f), new Rotation(0f, 0f, 2.6221168f)),
                new(new Position(-985.9121f, -2708.3604f, 13.32312f), new Rotation(0f, 0f, 2.6715908f)),
                new(new Position(-982.7868f, -2709.2966f, 13.339966f), new Rotation(0f, 0f, 2.7705386f)),
                new(new Position(-979.60876f, -2710.167f, 13.339966f), new Rotation(0f, 0f, 2.8694863f)),
                new(new Position(-976.5099f, -2710.8923f, 13.356812f), new Rotation(0f, 0f, 2.968434f)),
                new(new Position(-973.2132f, -2710.7078f, 13.356812f), new Rotation(0f, 0f, -3.1168559f)),
                new(new Position(-969.7319f, -2710.932f, 13.339966f), new Rotation(0f, 0f, -3.017908f)),
                new(new Position(-966.5143f, -2711.0242f, 13.339966f), new Rotation(0f, 0f, -3.017908f)),
                new(new Position(-963.37585f, -2710.3516f, 13.32312f), new Rotation(0f, 0f, -2.91896f)),
                new(new Position(-960.0791f, -2709.8242f, 13.32312f), new Rotation(0f, 0f, -2.968434f)),
                new(new Position(-962.04395f, -2699.4197f, 13.32312f), new Rotation(0f, 0f, -0.54421294f)),
                new(new Position(-964.8132f, -2697.6265f, 13.32312f), new Rotation(0f, 0f, -0.54421294f)),
                new(new Position(-967.75385f, -2696.0308f, 13.32312f), new Rotation(0f, 0f, -0.49473903f)),
                new(new Position(-970.53625f, -2694.1978f, 13.32312f), new Rotation(0f, 0f, -0.49473903f)),
                new(new Position(-973.3582f, -2692.7078f, 13.32312f), new Rotation(0f, 0f, -0.49473903f)),
                new(new Position(-976.3516f, -2690.9934f, 13.32312f), new Rotation(0f, 0f, -0.49473903f)),
                new(new Position(-979.2923f, -2689.1077f, 13.339966f), new Rotation(0f, 0f, -0.44526514f)),
                new(new Position(-1038.422f, -2678.611f, 13.32312f), new Rotation(0f, 0f, 2.5231688f)),
                new(new Position(-1041.534f, -2676.4219f, 13.339966f), new Rotation(0f, 0f, 2.473695f)),
                new(new Position(-1044.4747f, -2674.4702f, 13.339966f), new Rotation(0f, 0f, 2.424221f)),
                new(new Position(-1046.2946f, -2672.1494f, 13.32312f), new Rotation(0f, 0f, 2.3252733f)),
                new(new Position(-1048.9055f, -2669.8022f, 13.32312f), new Rotation(0f, 0f, 2.2263255f)),
                new(new Position(-1050.6329f, -2666.7825f, 13.32312f), new Rotation(0f, 0f, 2.1273777f)),
                new(new Position(-1052.2153f, -2664f, 13.339966f), new Rotation(0f, 0f, 1.9789561f)),
                new(new Position(-1053.7979f, -2660.9539f, 13.32312f), new Rotation(0f, 0f, 1.9294822f)),
                new(new Position(-1055.1033f, -2658.066f, 13.339966f), new Rotation(0f, 0f, 1.8800083f)),
                new(new Position(-1056f, -2655.0857f, 13.32312f), new Rotation(0f, 0f, 1.8800083f)),
                new(new Position(-1046.9011f, -2650.7078f, 13.339966f), new Rotation(0f, 0f, -0.44526514f)),
                new(new Position(-1043.9209f, -2652.2637f, 13.32312f), new Rotation(0f, 0f, -0.49473903f)),
                new(new Position(-1040.8088f, -2653.6748f, 13.32312f), new Rotation(0f, 0f, -0.54421294f)),
                new(new Position(-1037.8418f, -2655.1648f, 13.32312f), new Rotation(0f, 0f, -0.49473903f)),
                new(new Position(-1035.0989f, -2657.0242f, 13.32312f), new Rotation(0f, 0f, -0.49473903f)),
                new(new Position(-1032.2373f, -2658.7517f, 13.339966f), new Rotation(0f, 0f, -0.49473903f)),
                new(new Position(-1028.9802f, -2660.4922f, 13.32312f), new Rotation(0f, 0f, -0.54421294f))
            }),
        new SpawnLocation(1, "Metro Station: Davis", new Position(100.9774f, -1713.516f, 30.11263f),
            new Rotation(0, 0, 53.93463f),
            new List<LocationData>
            {
                new(new Position(20.07033f, -1776f, 28.824951f), new Rotation(0f, 0f, -2.2263255f)),
                new(new Position(22.312088f, -1773.6263f, 28.824951f), new Rotation(0f, 0f, -2.2757995f)),
                new(new Position(29.762638f, -1764.6461f, 28.824951f), new Rotation(0f, 0f, -2.2263255f)),
                new(new Position(31.52967f, -1762.3385f, 28.808105f), new Rotation(0f, 0f, -2.2757995f)),
                new(new Position(33.61319f, -1760.0176f, 28.808105f), new Rotation(0f, 0f, -2.2757995f)),
                new(new Position(35.696705f, -1757.5648f, 28.808105f), new Rotation(0f, 0f, -2.2757995f)),
                new(new Position(39.534065f, -1752.844f, 28.808105f), new Rotation(0f, 0f, -2.2263255f)),
                new(new Position(47.630768f, -1743.2968f, 28.808105f), new Rotation(0f, 0f, -2.2263255f)),
                new(new Position(49.56923f, -1740.989f, 28.808105f), new Rotation(0f, 0f, -2.2757995f)),
                new(new Position(56.386814f, -1733.1956f, 28.808105f), new Rotation(0f, 0f, -2.2263255f)),
                new(new Position(58.54945f, -1730.4923f, 28.808105f), new Rotation(0f, 0f, -2.2757995f)),
                new(new Position(65.45934f, -1722.2373f, 28.808105f), new Rotation(0f, 0f, -2.3252733f)),
                new(new Position(67.33187f, -1719.9297f, 28.808105f), new Rotation(0f, 0f, -2.2757995f)),
                new(new Position(69.56044f, -1717.7803f, 28.808105f), new Rotation(0f, 0f, -2.2757995f)),
                new(new Position(50.874725f, -1717.9912f, 28.808105f), new Rotation(0f, 0f, 0.8410563f)),
                new(new Position(48.725273f, -1720.1934f, 28.808105f), new Rotation(0f, 0f, 0.8410563f)),
                new(new Position(46.615387f, -1722.567f, 28.808105f), new Rotation(0f, 0f, 0.8410563f)),
                new(new Position(44.76923f, -1725.0593f, 28.808105f), new Rotation(0f, 0f, 0.8410563f)),
                new(new Position(42.778023f, -1727.4198f, 28.808105f), new Rotation(0f, 0f, 0.7915824f)),
                new(new Position(40.786816f, -1729.833f, 28.808105f), new Rotation(0f, 0f, 0.8410563f)),
                new(new Position(38.49231f, -1731.9692f, 28.808105f), new Rotation(0f, 0f, 0.8410563f)),
                new(new Position(36.685715f, -1734.3561f, 28.808105f), new Rotation(0f, 0f, 0.8410563f)),
                new(new Position(34.77363f, -1736.9011f, 28.808105f), new Rotation(0f, 0f, 0.8410563f)),
                new(new Position(32.69011f, -1739.0242f, 28.808105f), new Rotation(0f, 0f, 0.8410563f)),
                new(new Position(30.75165f, -1741.2263f, 28.808105f), new Rotation(0f, 0f, 0.8410563f)),
                new(new Position(28.64176f, -1743.5472f, 28.808105f), new Rotation(0f, 0f, 0.8410563f)),
                new(new Position(26.821978f, -1745.9736f, 28.808105f), new Rotation(0f, 0f, 0.8410563f)),
                new(new Position(24.712088f, -1748.4f, 28.808105f), new Rotation(0f, 0f, 0.8410563f)),
                new(new Position(22.852747f, -1750.7076f, 28.808105f), new Rotation(0f, 0f, 0.7915824f)),
                new(new Position(20.610989f, -1753.0022f, 28.808105f), new Rotation(0f, 0f, 0.7915824f)),
                new(new Position(19.068132f, -1755.389f, 28.808105f), new Rotation(0f, 0f, 0.8410563f)),
                new(new Position(16.918682f, -1757.9209f, 28.808105f), new Rotation(0f, 0f, 0.7915824f)),
                new(new Position(14.742858f, -1760.1099f, 28.808105f), new Rotation(0f, 0f, 0.8410563f)),
                new(new Position(12.936264f, -1762.5099f, 28.808105f), new Rotation(0f, 0f, 0.8410563f)),
                new(new Position(10.958241f, -1764.8176f, 28.808105f), new Rotation(0f, 0f, 0.8410563f)),
                new(new Position(9.0593405f, -1767.2439f, 28.808105f), new Rotation(0f, 0f, 0.8410563f)),
                new(new Position(4.76044f, -1762.7472f, 28.79126f), new Rotation(0f, 0f, -2.2757995f)),
                new(new Position(7.094506f, -1760.611f, 28.808105f), new Rotation(0f, 0f, -2.2757995f)),
                new(new Position(8.887913f, -1758.2638f, 28.808105f), new Rotation(0f, 0f, -2.2263255f)),
                new(new Position(11.037363f, -1756.1011f, 28.808105f), new Rotation(0f, 0f, -2.2757995f)),
                new(new Position(13.041759f, -1753.6879f, 28.808105f), new Rotation(0f, 0f, -2.2263255f)),
                new(new Position(14.927473f, -1751.3407f, 28.808105f), new Rotation(0f, 0f, -2.2757995f)),
                new(new Position(16.97143f, -1748.8484f, 28.808105f), new Rotation(0f, 0f, -2.2263255f)),
                new(new Position(19.134066f, -1746.6461f, 28.808105f), new Rotation(0f, 0f, -2.2263255f)),
                new(new Position(20.690111f, -1744.0088f, 28.808105f), new Rotation(0f, 0f, -2.2757995f)),
                new(new Position(22.839561f, -1741.7406f, 28.808105f), new Rotation(0f, 0f, -2.2757995f)),
                new(new Position(24.883516f, -1739.5121f, 28.808105f), new Rotation(0f, 0f, -2.2757995f)),
                new(new Position(26.927473f, -1737.3099f, 28.808105f), new Rotation(0f, 0f, -2.2263255f)),
                new(new Position(28.575825f, -1734.5802f, 28.808105f), new Rotation(0f, 0f, -2.2263255f)),
                new(new Position(30.553848f, -1732.4308f, 28.808105f), new Rotation(0f, 0f, -2.2757995f)),
                new(new Position(32.769234f, -1730.1758f, 28.808105f), new Rotation(0f, 0f, -2.2757995f)),
                new(new Position(34.562637f, -1727.5912f, 28.808105f), new Rotation(0f, 0f, -2.2757995f)),
                new(new Position(36.61978f, -1725.3495f, 28.808105f), new Rotation(0f, 0f, -2.2263255f)),
                new(new Position(38.531868f, -1722.8835f, 28.808105f), new Rotation(0f, 0f, -2.2757995f)),
                new(new Position(40.72088f, -1720.6945f, 28.808105f), new Rotation(0f, 0f, -2.2263255f)),
                new(new Position(42.435165f, -1718.3077f, 28.808105f), new Rotation(0f, 0f, -2.2757995f)),
                new(new Position(44.67692f, -1716.145f, 28.808105f), new Rotation(0f, 0f, -2.2757995f)),
                new(new Position(46.496704f, -1713.5604f, 28.808105f), new Rotation(0f, 0f, -2.2757995f)),
                new(new Position(10.549451f, -1731.4681f, 28.808105f), new Rotation(0f, 0f, 0.8410563f)),
                new(new Position(8.505495f, -1733.9473f, 28.808105f), new Rotation(0f, 0f, 0.8410563f)),
                new(new Position(6.3164835f, -1736.0308f, 28.808105f), new Rotation(0f, 0f, 0.8410563f)),
                new(new Position(4.3516483f, -1738.378f, 28.808105f), new Rotation(0f, 0f, 0.8410563f)),
                new(new Position(2.5318682f, -1740.8572f, 28.808105f), new Rotation(0f, 0f, 0.8410563f)),
                new(new Position(0.105494514f, -1743.1912f, 28.808105f), new Rotation(0f, 0f, 0.8410563f)),
                new(new Position(-3.4153862f, -1748.044f, 28.808105f), new Rotation(0f, 0f, 0.8410563f)),
                new(new Position(-5.485714f, -1750.378f, 28.808105f), new Rotation(0f, 0f, 0.8410563f)),
                new(new Position(-7.4373627f, -1752.6593f, 28.808105f), new Rotation(0f, 0f, 0.8410563f)),
                new(new Position(-11.736263f, -1748.2946f, 28.808105f), new Rotation(0f, 0f, -2.2757995f)),
                new(new Position(-9.942856f, -1745.8945f, 28.808105f), new Rotation(0f, 0f, -2.2757995f)),
                new(new Position(-7.661537f, -1743.6132f, 28.808105f), new Rotation(0f, 0f, -2.2757995f)),
                new(new Position(-5.7098885f, -1741.3846f, 28.808105f), new Rotation(0f, 0f, -2.2757995f)),
                new(new Position(-35.221977f, -1733.2616f, 28.808105f), new Rotation(0f, 0f, 0.3957912f)),
                new(new Position(-32.320877f, -1732.0088f, 28.79126f), new Rotation(0f, 0f, 0.34631732f)),
                new(new Position(-29.70989f, -1730.5187f, 28.79126f), new Rotation(0f, 0f, 0.34631732f)),
                new(new Position(-26.76923f, -1729.4374f, 28.79126f), new Rotation(0f, 0f, 0.34631732f)),
                new(new Position(-23.881317f, -1728.422f, 28.79126f), new Rotation(0f, 0f, 0.34631732f)),
                new(new Position(-21.006592f, -1727.0505f, 28.808105f), new Rotation(0f, 0f, 0.34631732f)),
                new(new Position(-6.1318665f, -1721.0901f, 28.808105f), new Rotation(0f, 0f, 0.3957912f)),
                new(new Position(-3.3362617f, -1719.811f, 28.79126f), new Rotation(0f, 0f, 0.34631732f)),
                new(new Position(-0.54066086f, -1718.8352f, 28.79126f), new Rotation(0f, 0f, 0.3957912f)),
                new(new Position(2.2021978f, -1717.2792f, 28.79126f), new Rotation(0f, 0f, 0.34631732f)),
                new(new Position(5.1956043f, -1716.2902f, 28.79126f), new Rotation(0f, 0f, 0.34631732f)),
                new(new Position(8.096704f, -1715.0901f, 28.79126f), new Rotation(0f, 0f, 0.34631732f)),
                new(new Position(10.997803f, -1713.9956f, 28.79126f), new Rotation(0f, 0f, 0.3957912f)),
                new(new Position(13.806594f, -1712.9275f, 28.79126f), new Rotation(0f, 0f, 0.34631732f)),
                new(new Position(16.417583f, -1711.7406f, 28.79126f), new Rotation(0f, 0f, 0.34631732f)),
                new(new Position(19.476923f, -1710.3693f, 28.79126f), new Rotation(0f, 0f, 0.3957912f)),
                new(new Position(22.285715f, -1709.2087f, 28.79126f), new Rotation(0f, 0f, 0.3957912f)),
                new(new Position(25.081318f, -1708.0747f, 28.79126f), new Rotation(0f, 0f, 0.34631732f)),
                new(new Position(27.863737f, -1706.9802f, 28.79126f), new Rotation(0f, 0f, 0.44526514f)),
                new(new Position(30.725277f, -1705.556f, 28.79126f), new Rotation(0f, 0f, 0.3957912f)),
                new(new Position(33.626373f, -1704.6989f, 28.79126f), new Rotation(0f, 0f, 0.34631732f))
            })
    };

    private readonly WeaponModule _weaponModule;

    public CharacterSpawnModule(ILogger<CharacterSpawnModule> logger, WeaponModule weaponModule, AmmoModule ammoModule,
        PhoneModule phoneModule, DeathModule deathModule, FactionGroupService factionGroupService, ClothingModule clothingModule)
    {
        _logger = logger;
        _weaponModule = weaponModule;
        _ammoModule = ammoModule;
        _phoneModule = phoneModule;
        _deathModule = deathModule;
        _factionGroupService = factionGroupService;
        _clothingModule = clothingModule;
    }

    public async Task Spawn(ServerPlayer player, Position position, Rotation rotation, int dimension)
    {
        if (!player.Exists)
        {
            return;
        }

        player.Spawn(position, 0);

        player.Rotation = rotation;
        player.Dimension = dimension;
        player.IsSpawned = true;
        player.Health = player.CharacterModel.Health;
        player.Armor = player.CharacterModel.Armor;

        player.Model = player.CharacterModel.Gender switch
        {
            GenderType.MALE => (uint)PedModel.FreemodeMale01,
            GenderType.FEMALE => (uint)PedModel.FreemodeFemale01,
            _ => player.Model
        };

        player.EmitLocked("character:spawn", player.CharacterModel);
        player.EmitLocked("player:setinhouse", player.Dimension != 0);
        player.SetSyncedMetaData("ID", player.Id);
        player.UpdateMoneyUi();

        _clothingModule.UpdateClothes(player);
        
        if (player.IsAduty)
        {
            player.SetSyncedMetaData("CHARACTER_NAME", player.AccountName);
            player.SetSyncedMetaData("NAMECOLOR", "~r~");
        }
        else
        {
            player.SetSyncedMetaData("CHARACTER_NAME", player.CharacterModel.Name);
            player.SetSyncedMetaData("NAMECOLOR", "~w~");
        }

        var factionGroup = await _factionGroupService.GetByCharacter(player.CharacterModel.Id);
        player.SetSyncedMetaData("IS_MEDIC", factionGroup?.FactionType == FactionType.FIRE_DEPARTMENT);

        foreach (var item in player.CharacterModel.InventoryModel.Items)
        {
            if (WeaponModule.IsItemWeapon(item.CatalogItemModelId))
            {
                var itemWeapon = (ItemWeaponModel)item;
                _weaponModule.Give(player, WeaponModule.GetModelFromId(item.CatalogItemModelId), false,
                    itemWeapon.Amount);
                if (itemWeapon.ComponentHashes == null)
                {
                    continue;
                }

                foreach (var componentHash in itemWeapon.ComponentHashes)
                {
                    await AttachmentModule.AddWeaponComponent(player, itemWeapon, componentHash);
                }
            }
            else if (AmmoModule.IsItemAmmo(item.CatalogItemModelId))
            {
                _ammoModule.Give(player, item.CatalogItemModelId, item.Amount);
            }
            else if (item.CatalogItemModelId == ItemCatalogIds.PHONE)
            {
                await _phoneModule.SetActive(player, item.Id);
            }
            else if (item.CatalogItemModelId == ItemCatalogIds.HANDCUFF && item.ItemState == ItemState.FORCE_EQUIPPED)
            {
                player.Cuffed = true;
            }
        }

        if (player.CharacterModel.DeathState == DeathState.DEAD)
        {
            await _deathModule.SetPlayerDead(player);
        }
    }

    public SpawnLocation GetSpawn(int id)
    {
        return _spawns.First(s => s.Id == id);
    }

    public List<SpawnLocation> GetSpawns()
    {
        return _spawns;
    }

    public LocationData GetFreeVehicleLocation(SpawnLocation spawn)
    {
        var locationData = spawn.VehicleLocations[0];

        foreach (var vehiclePosition in spawn.VehicleLocations.Shuffle())
        {
            if (Alt.GetAllVehicles().FirstOrDefault(v => v.Position.Distance(vehiclePosition.Position) <= 2) == null)
            {
                locationData = vehiclePosition;
            }
        }

        return locationData;
    }
}