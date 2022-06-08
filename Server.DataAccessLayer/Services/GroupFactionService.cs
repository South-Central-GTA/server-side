using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.Core.Abstractions.ScriptStrategy;
using Server.DataAccessLayer.Context;
using Server.DataAccessLayer.Services.Base;
using Server.Database.Enums;
using Server.Database.Models.Group;

namespace Server.DataAccessLayer.Services;

public class GroupFactionService
    : BaseService<FactionGroupModel>, ITransientScript
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

    public GroupFactionService(IDbContextFactory<DatabaseContext> dbContextFactory)
        : base(dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<FactionGroupModel?> GetFactionByCharacter(int characterId)
    {
        var ownedFaction = await GetByOwner(characterId);
        if (ownedFaction != null)
        {
            return ownedFaction;
        }

        var memberFaction = await GetByMember(characterId);

        return memberFaction;
    }

    public override async Task<FactionGroupModel?> GetByKey(object id)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.FactionGroups
                              .Include(group => group.Members)
                              .ThenInclude(member => member.CharacterModel)
                              .Include(group => group.Ranks)
                              .FirstOrDefaultAsync(group => group.Id == (int)id);
    }

    public override async Task<List<FactionGroupModel>> GetAll()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.FactionGroups
                              .Include(group => group.Members)
                              .ThenInclude(member => member.CharacterModel)
                              .Include(group => group.Ranks)
                              .ToListAsync();
    }

    public async Task<FactionGroupModel?> GetByOwner(int characterId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.FactionGroups
                              .Include(group => group.Members)
                              .ThenInclude(member => member.CharacterModel)
                              .Include(group => group.Ranks)
                              .FirstOrDefaultAsync(group =>
                                                       group.Members != null &&
                                                       group.Members.Any(
                                                           m => m.CharacterModelId == characterId && m.Owner));
    }

    public async Task<FactionGroupModel?> GetByMember(int characterId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.FactionGroups
                              .Include(group => group.Members)
                              .ThenInclude(member => member.CharacterModel)
                              .Include(group => group.Ranks)
                              .FirstOrDefaultAsync(group =>
                                                       group.Members != null &&
                                                       group.Members.Any(
                                                           m => m.CharacterModelId == characterId && !m.Owner));
    }

    public override async Task<List<FactionGroupModel>> Where(Expression<Func<FactionGroupModel, bool>> expression)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.FactionGroups
                              .Include(group => group.Members)
                              .ThenInclude(member => member.CharacterModel)
                              .Include(group => group.Ranks)
                              .Where(expression)
                              .ToListAsync();
    }

    public override async Task<FactionGroupModel?> Find(Expression<Func<FactionGroupModel, bool>> expression)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.FactionGroups
                              .Include(group => group.Members)
                              .ThenInclude(member => member.CharacterModel)
                              .Include(group => group.Ranks)
                              .FirstOrDefaultAsync(expression);
    }
}