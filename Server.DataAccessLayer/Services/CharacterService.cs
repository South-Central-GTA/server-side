using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Context;
using Server.DataAccessLayer.Services.Base;
using Server.Database.Models;
using Server.Database.Models.Character;

namespace Server.DataAccessLayer.Services;

public class CharacterService
    : BaseService<CharacterModel>, ITransientScript
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

    public CharacterService(
        IDbContextFactory<DatabaseContext> dbContextFactory)
        : base(dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<CharacterModel?> GetByKey(int characterId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Characters
                              .Include(character => character.Licenses)
                              .Include(character => character.AccountModel)
                              .Include(character => character.FaceFeaturesModel)
                              .Include(character => character.AppearancesModel)
                              .Include(character => character.TattoosModel)
                              .Include(character => character.InventoryModel)
                              .ThenInclude(inventory => inventory.Items)
                              .ThenInclude(item => item.CatalogItemModel)
                              .Include(character => character.JobModel)
                              .FirstOrDefaultAsync(character => character.Id == characterId);
    }

    public async Task<List<CharacterModel>> GetAllFromAccount(AccountModel accountModel)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Characters
                              .Include(character => character.Licenses)
                              .Include(character => character.AccountModel)
                              .Include(character => character.FaceFeaturesModel)
                              .Include(character => character.AppearancesModel)
                              .Include(character => character.TattoosModel)
                              .Include(character => character.InventoryModel)
                              .ThenInclude(inventory => inventory.Items)
                              .ThenInclude(item => item.CatalogItemModel)
                              .Include(character => character.JobModel)
                              .Where(e => e.AccountModel == accountModel).ToListAsync();
    }

    public override async Task<List<CharacterModel>> GetAll()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Characters
                              .Include(character => character.Licenses)
                              .Include(character => character.AccountModel)
                              .Include(character => character.FaceFeaturesModel)
                              .Include(character => character.AppearancesModel)
                              .Include(character => character.TattoosModel)
                              .Include(character => character.InventoryModel)
                              .ThenInclude(inventory => inventory.Items)
                              .ThenInclude(item => item.CatalogItemModel)
                              .Include(character => character.JobModel)
                              .ToListAsync();
    }

    public async Task Update(ServerPlayer player)
    {
        player.CharacterModel = player.CharacterModel;

        var position = player.IsInVehicle ? player.Vehicle.Position : player.Position;

        player.CharacterModel.PositionX = position.X;
        player.CharacterModel.PositionY = position.Y;
        player.CharacterModel.PositionZ = position.Z;

        player.CharacterModel.Roll = player.Rotation.Roll;
        player.CharacterModel.Pitch = player.Rotation.Pitch;
        player.CharacterModel.Yaw = player.Rotation.Yaw;

        player.CharacterModel.Dimension = player.Dimension;
        player.CharacterModel.Health = player.Health;
        player.CharacterModel.Armor = player.Armor;

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        player.CharacterModel.LastUsage = DateTime.Now;

        dbContext.Entry(player.CharacterModel.InventoryModel).State = EntityState.Unchanged;

        var entityEntry = dbContext.Characters.Update(player.CharacterModel);
        await dbContext.SaveChangesAsync();
    }
}