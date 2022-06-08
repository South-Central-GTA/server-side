using System.Linq;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using Microsoft.Extensions.Logging;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Group;
using Server.Database.Models.Inventory;
using Server.Helper;
using Server.Modules.Houses;

namespace Server.Modules.Group;

public class GroupModule
    : ITransientScript
{
    private readonly DeliveryService _deliveryService;
    private readonly GroupMemberService _groupMemberService;
    private readonly GroupRankService _groupRankService;

    private readonly GroupService _groupService;

    private readonly HouseModule _houseModule;
    private readonly HouseService _houseService;
    private readonly InventoryService _inventoryService;
    private readonly ItemService _itemService;
    private readonly ILogger<GroupModule> _logger;
    private readonly MailAccountService _mailAccountService;
    private readonly OrderedVehicleService _orderedVehicleService;
    private readonly Serializer _serializer;
    private readonly VehicleService _vehicleService;

    public GroupModule(
        ILogger<GroupModule> logger,
        Serializer serializer,
        GroupService groupService,
        GroupMemberService groupMemberService,
        GroupRankService groupRankService,
        HouseService houseService,
        ItemService itemService,
        VehicleService vehicleService,
        InventoryService inventoryService,
        OrderedVehicleService orderedVehicleService,
        MailAccountService mailAccountService,
        DeliveryService deliveryService,
        HouseModule houseModule)
    {
        _logger = logger;
        _serializer = serializer;

        _groupService = groupService;
        _groupMemberService = groupMemberService;
        _groupRankService = groupRankService;
        _houseService = houseService;
        _itemService = itemService;
        _vehicleService = vehicleService;
        _inventoryService = inventoryService;
        _orderedVehicleService = orderedVehicleService;
        _mailAccountService = mailAccountService;
        _deliveryService = deliveryService;

        _houseModule = houseModule;
    }

    public async Task<bool> IsPlayerInGroupType(ServerPlayer player, GroupType type)
    {
        var isInGroup = false;

        var existingMember = await _groupMemberService.Find(m => m.CharacterModelId == player.CharacterModel.Id);
        if (existingMember != null)
        {
            isInGroup = existingMember.GroupModel.GroupType == type;
        }

        return isInGroup;
    }

    public async Task<bool> IsPlayerInAnyGroup(ServerPlayer player)
    {
        var existingMember = await _groupMemberService.Find(m => m.CharacterModelId == player.CharacterModel.Id);
        return existingMember != null;
    }

    public async Task<bool> IsPlayerInGroup(ServerPlayer player, int groupId)
    {
        var existingMember =
            await _groupMemberService.Find(m => m.CharacterModelId == player.CharacterModel.Id &&
                                                m.GroupModelId == groupId);
        return existingMember != null;
    }

    public async Task<GroupMemberModel?> GetGroupMember(ServerPlayer player, int groupId)
    {
        return await _groupMemberService.Find(m => m.CharacterModelId == player.CharacterModel.Id &&
                                                   m.GroupModelId == groupId);
    }

    public async Task<GroupModel?> CreateGroup(GroupType groupType, string name)
    {
        var existingGroup = await _groupService.Find(g => g.Name == name);
        if (existingGroup != null)
        {
            return null;
        }

        GroupModel newGroupModel;

        switch (groupType)
        {
            case GroupType.GROUP:
                newGroupModel = new GroupModel(name);
                break;
            case GroupType.FACTION:
                newGroupModel = new FactionGroupModel(name, FactionType.CITIZEN);
                break;
            case GroupType.COMPANY:
                newGroupModel = new CompanyGroupModel(name);
                break;
            default:
                newGroupModel = new GroupModel(name);
                break;
        }

        return await _groupService.Add(newGroupModel);
    }

    public async Task<bool> Invite(ServerPlayer? inviter, GroupModel groupModel, ServerPlayer target)
    {
        var existingMember = await _groupMemberService.Find(m => m.CharacterModelId == target.CharacterModel.Id);
        if (existingMember != null)
        {
            // Add check for group.GroupType here if we have multiple groups that can be combined.
            switch (groupModel.GroupType)
            {
                case GroupType.FACTION:
                    target.SendNotification("Dein Charakter befindet sich schon in einer Fraktion.",
                                            NotificationType.ERROR);
                    break;
                case GroupType.COMPANY:
                    target.SendNotification("Dein Charakter befindet sich schon in einem Unternehmen.",
                                            NotificationType.ERROR);
                    break;
            }

            if (inviter is { Exists: true })
            {
                inviter.SendNotification("Der Charakter ist schon in einer Gruppe.", NotificationType.ERROR);
            }

            return false;
        }

        var member = new GroupMemberModel
        {
            GroupModelId = groupModel.Id, CharacterModelId = target.CharacterModel.Id, RankLevel = 1
        };

        target.SendNotification("Du hast die Einladung angenommen.", NotificationType.SUCCESS);

        if (inviter is { Exists: false })
        {
            inviter.SendNotification("Die Einladung wurde angenommen.", NotificationType.SUCCESS);
        }

        await _groupMemberService.Add(member);

        await CreateMemberInventory(target, groupModel);
        await UpdateUi(target);
        await UpdateGroupUi(groupModel);

        return true;
    }

    public async Task Kick(ServerPlayer player, GroupMemberModel groupMemberModel)
    {
        await _groupMemberService.Remove(groupMemberModel);

        var target = Alt.GetAllPlayers().FindPlayerByCharacterId(groupMemberModel.CharacterModelId);
        if (target == null)
        {
            player.SendNotification("Der Spieler konnte nicht gefunden werden.", NotificationType.ERROR);
            return;
        }

        target.SendNotification(
            $"Du wurdest von {player.CharacterModel.Name} aus der Gruppe {groupMemberModel.GroupModel.Name} entfernt.",
            NotificationType.WARNING);
        await UpdateUi(target);

        await UpdateGroupUi(groupMemberModel.GroupModel);
        await RemoveMemberInventory(target, groupMemberModel.GroupModel);

        player.SendNotification("Du hast den Charakter aus deiner Gruppe freigesetzt.", NotificationType.INFO);
    }

    public async Task AdminKick(ServerPlayer player, ServerPlayer adminPlayer, int groupId)
    {
        var group = await _groupService.GetByKey(groupId);
        if (group == null)
        {
            return;
        }

        var member = group.Members.Find(m => m.CharacterModelId == player.CharacterModel.Id);
        if (member == null)
        {
            adminPlayer.SendNotification("Es wurde kein Member der Gruppe gefunden.", NotificationType.ERROR);
            return;
        }

        await _groupMemberService.Remove(member);

        player.SendNotification(
            $"Du wurdest von administrativ von {adminPlayer.AccountName} aus der Gruppe {group.Name} entfernt.",
            NotificationType.WARNING);

        await UpdateUi(player);

        await UpdateGroupUi(group);

        await RemoveMemberInventory(player, group);

        adminPlayer.SendNotification($"Du hast {player.CharacterModel.Name} aus der Gruppe freigesetzt.",
                                     NotificationType.INFO);
    }

    public async Task Leave(ServerPlayer player, GroupModel groupModel)
    {
        var groupMember = await _groupMemberService.Find(m => m.GroupModelId == groupModel.Id
                                                              && m.CharacterModelId == player.CharacterModel.Id);

        await _groupMemberService.Remove(groupMember);
        await UpdateGroupUi(groupModel);
        await RemoveMemberInventory(player, groupModel);

        // We have to seperate update the user because he is no longer in the group.
        await UpdateUi(player);
    }

    public async Task DeleteGroup(GroupModel groupModel)
    {
        var ownedGroupHouses = await _houseService.Where(h => h.GroupModelId == groupModel.Id);
        foreach (var ownedHouse in ownedGroupHouses)
        {
            if (ownedHouse.Inventory != null)
            {
                await _itemService.RemoveRange(ownedHouse.Inventory.Items);
            }

            await _houseModule.ResetOwner(ownedHouse);
        }

        var ownedGroupVehicles = await _vehicleService.Where(v => v.GroupModelOwnerId == groupModel.Id);
        foreach (var ownedVehicle in ownedGroupVehicles)
        {
            if (ownedVehicle.InventoryModel != null)
            {
                await _itemService.RemoveRange(ownedVehicle.InventoryModel.Items);
                await _inventoryService.Remove(ownedVehicle.InventoryModel);
            }

            var vehicle = Alt.GetAllVehicles().FindByDbId(ownedVehicle.Id);
            if (vehicle is { Exists: true })
            {
                await vehicle.RemoveAsync();
            }

            // Update possible open deliveries
            var deliveries = await _deliveryService.Where(d => d.PlayerVehicleModelId == ownedVehicle.Id);
            foreach (var delivery in deliveries)
            {
                delivery.PlayerVehicleModelId = null;
            }

            await _vehicleService.Remove(ownedVehicle);
        }

        if (groupModel.Members != null)
        {
            await _groupMemberService.RemoveRange(groupModel.Members);
        }

        if (groupModel.Ranks != null)
        {
            await _groupRankService.RemoveRange(groupModel.Ranks);
        }

        var orderedVehicles = await _orderedVehicleService.Where(h => h.GroupModelId == groupModel.Id);
        if (orderedVehicles != null)
        {
            await _orderedVehicleService.RemoveRange(orderedVehicles);
        }

        var mailAccount = await _mailAccountService.GetByGroup(groupModel.Id);
        await _mailAccountService.Remove(mailAccount);

        var deletedGroup = await _groupService.Remove(groupModel);
        await UpdateGroupUi(deletedGroup);
    }

    public async Task SetSalary(ServerPlayer player, ServerPlayer target, GroupModel groupModel, uint salary)
    {
        var groupMember =
            await _groupMemberService.Find(m => m.GroupModelId == groupModel.Id &&
                                                m.CharacterModelId == target.CharacterModel.Id);
        if (groupMember == null)
        {
            player.SendNotification("Der Charakter ist nicht in deiner Gruppe. (Unternehmen, Fraktion)",
                                    NotificationType.ERROR);
            return;
        }

        groupMember.Salary = salary;

        await _groupMemberService.Update(groupMember);

        player.SendNotification($"Du hast das Gehalt erfolgreich auf ${salary} gesetzt.", NotificationType.INFO);
    }

    public async Task CreateRank(GroupModel groupModel, string name)
    {
        var ranks = await _groupRankService.Where(r => r.GroupModelId == groupModel.Id);
        var newLevel = ranks.Max(r => r.Level) + 1;

        await _groupRankService.Add(new GroupRankModel { GroupModelId = groupModel.Id, Name = name, Level = newLevel });
    }

    public async Task UpdateUi(ServerPlayer player)
    {
        var groups = await _groupService.Where(g => g.Members.Any(m => m.CharacterModelId == player.CharacterModel.Id));

        var allGroups = groups;
        var companyGroup = (CompanyGroupModel)groups.FirstOrDefault(g => g.GroupType == GroupType.COMPANY);
        var factionGroup = (FactionGroupModel)groups.FirstOrDefault(g => g.GroupType == GroupType.FACTION);
        groups = groups.FindAll(g => g.GroupType == GroupType.GROUP);

        player.EmitLocked("group:setup", allGroups, groups, companyGroup, factionGroup);
    }

    public async Task UpdateGroupUi(GroupModel groupModel)
    {
        foreach (var serverPlayer in groupModel.Members.Select(groupMember =>
                                                                   Alt.GetAllPlayers()
                                                                      .FindPlayerByCharacterId(
                                                                          groupMember.CharacterModelId))
                                               .Where(serverPlayer => serverPlayer != null))
        {
            await UpdateUi(serverPlayer);
        }
    }

    public async Task RenameRank(int groupId, int level, string name)
    {
        var groupRank = await _groupRankService.Find(r => r.GroupModelId == groupId && r.Level == level);
        if (groupRank == null)
        {
            return;
        }

        groupRank.Name = name;

        await _groupRankService.Update(groupRank);
    }

    public async Task<bool> DeleteRank(GroupModel groupModel, int level)
    {
        for (var i = 0; i < groupModel.Members.Count; i++)
        {
            var member = groupModel.Members[i];
            if (member.RankLevel >= level)
            {
                return false;
            }
        }

        var rank = await _groupRankService.Find(r => r.GroupModelId == groupModel.Id && r.Level == level);
        if (rank == null)
        {
            return false;
        }

        await _groupRankService.Remove(rank);
        return true;
    }

    public async Task<bool> AddRankPermission(int groupId, int level, GroupPermission groupPermission)
    {
        var groupRank = await _groupRankService.Find(r => r.GroupModelId == groupId && r.Level == level);
        if (groupRank == null)
        {
            return false;
        }

        groupRank.GroupPermission |= groupPermission;

        await _groupRankService.Update(groupRank);
        return true;
    }

    public async Task<bool> RemoveRankPermission(int groupId, int level, GroupPermission groupPermission)
    {
        var groupRank = await _groupRankService.Find(r => r.GroupModelId == groupId && r.Level == level);
        if (groupRank == null)
        {
            return false;
        }

        groupRank.GroupPermission &= ~groupPermission;

        await _groupRankService.Update(groupRank);
        return true;
    }

    public async Task SetMemberRank(ServerPlayer player, GroupModel groupModel, int characterId, uint level)
    {
        var groupMember =
            await _groupMemberService.Find(m => m.GroupModelId == groupModel.Id && m.CharacterModelId == characterId);
        if (groupMember == null)
        {
            player.SendNotification("Der Charakter ist nicht in deiner Gruppe. (Unternehmen, Fraktion)",
                                    NotificationType.ERROR);
            return;
        }

        var playerMember =
            await _groupMemberService.Find(m => m.GroupModelId == groupModel.Id &&
                                                m.CharacterModelId == player.CharacterModel.Id);
        if (!playerMember.Owner && playerMember.RankLevel < level)
        {
            player.SendNotification(
                $"Dein Charakter ist Level {playerMember.RankLevel} in der aktuellen Gruppe, du kannst kein höheres Level vergeben.",
                NotificationType.ERROR);
            return;
        }

        if (level < 1)
        {
            player.SendNotification("Das geringste Level ist eins (1).", NotificationType.ERROR);
            return;
        }

        if (groupModel.Ranks.Count < level)
        {
            player.SendNotification("So viele Ränge hat deine Gruppe nicht.", NotificationType.ERROR);
            return;
        }

        groupMember.RankLevel = level;

        await _groupMemberService.Update(groupMember);

        var target = Alt.GetAllPlayers().FindPlayerByCharacterId(characterId);
        if (target != null)
        {
            await UpdateUi(target);
        }

        player.SendNotification($"Du hast das Level des Charakters auf {level} gesetzt.", NotificationType.SUCCESS);
    }

    public async Task<bool> HasPermission(int characterId, int groupId, GroupPermission groupPermission)
    {
        var groupMember = await _groupMemberService.Find(m => m.GroupModelId == groupId
                                                              && m.CharacterModelId == characterId);
        if (groupMember == null)
        {
            return false;
        }

        if (groupMember.Owner)
        {
            return true;
        }

        var rank = groupMember.GroupModel?.Ranks.Find(r => r.Level == groupMember.RankLevel);
        if (rank == null)
        {
            return false;
        }

        return rank.GroupPermission.HasFlag(groupPermission);
    }

    public async Task GiveGroup(GroupMemberModel newOwner, GroupModel groupModel)
    {
        foreach (var groupMember in groupModel.Members)
        {
            groupMember.Owner = false;
            groupMember.RankLevel = 1;

            await _groupMemberService.Update(groupMember);
        }

        var member = groupModel.Members.Find(m => m.CharacterModelId == newOwner.CharacterModel.Id);
        if (member != null)
        {
            member.Owner = true;

            await _groupMemberService.Update(member);
        }

        await UpdateGroupUi(groupModel);
    }

    public bool IsOwner(ServerPlayer player, GroupModel groupModel)
    {
        return groupModel.Members.Any(m => m.CharacterModelId == player.CharacterModel.Id && m.Owner);
    }

    public async Task CreateMemberInventory(ServerPlayer player, GroupModel groupModel)
    {
        await _inventoryService.Add(new InventoryModel
        {
            InventoryType = InventoryType.GROUP_MEMBER,
            GroupCharacterId = player.CharacterModel.Id,
            GroupId = groupModel.Id,
            MaxWeight = 20,
            Name = player.CharacterModel.Name + "'s Gruppen Inventar"
        });
    }

    private async Task RemoveMemberInventory(ServerPlayer player, GroupModel groupModel)
    {
        var inventory = await _inventoryService
            .Find(i => i.GroupCharacterId == player.CharacterModel.Id && i.GroupId == groupModel.Id);
        if (inventory == null)
        {
            return;
        }

        await _itemService.RemoveRange(inventory.Items);
        await _inventoryService.Remove(inventory);
    }
}