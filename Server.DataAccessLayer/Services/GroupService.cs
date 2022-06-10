using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.Core.Abstractions.ScriptStrategy;
using Server.DataAccessLayer.Context;
using Server.DataAccessLayer.Services.Base;
using Server.Database.Models.Group;

namespace Server.DataAccessLayer.Services;

public class GroupService : BaseService<GroupModel>, ITransientScript
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

    public GroupService(IDbContextFactory<DatabaseContext> dbContextFactory) : base(dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<List<GroupModel>> GetGroupsByCharacter(int characterId)
    {
        var groups = new List<GroupModel>();

        var ownedGroups = await GetByOwner(characterId);
        if (ownedGroups != null)
        {
            groups.AddRange(ownedGroups);
        }

        var memberGroups = await GetByMember(characterId);
        if (memberGroups.Count != 0)
        {
            groups.AddRange(memberGroups);
        }

        return groups;
    }

    public override async Task<GroupModel?> GetByKey(object id)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Groups.Include(group => group.Members).ThenInclude(member => member.CharacterModel)
            .Include(group => group.Ranks).Include(group => group.Houses)
            .FirstOrDefaultAsync(group => group.Id == (int)id);
    }

    public override async Task<List<GroupModel>> GetAll()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Groups.Include(group => group.Members).ThenInclude(member => member.CharacterModel)
            .Include(group => group.Ranks).Include(group => group.Houses).ToListAsync();
    }

    public async Task<List<GroupModel>> GetByOwner(int characterId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Groups.Include(group => group.Members).ThenInclude(member => member.CharacterModel)
            .Include(group => group.Ranks)
            .Where(group => group.Members != null &&
                            group.Members.Any(m => m.CharacterModelId == characterId && m.Owner))
            .Include(group => group.Houses).ToListAsync();
    }

    public async Task<List<GroupModel>> GetByMember(int characterId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Groups.Include(group => group.Members).ThenInclude(member => member.CharacterModel)
            .Include(group => group.Ranks)
            .Where(group => group.Members != null &&
                            group.Members.Any(m => m.CharacterModelId == characterId && !m.Owner))
            .Include(group => group.Houses).ToListAsync();
    }

    public override async Task<List<GroupModel>> Where(Expression<Func<GroupModel, bool>> expression)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Groups.Include(group => group.Members).ThenInclude(member => member.CharacterModel)
            .Include(group => group.Ranks).Where(expression).Include(group => group.Houses).ToListAsync();
    }

    public override async Task<GroupModel?> Find(Expression<Func<GroupModel, bool>> expression)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Groups.Include(group => group.Members).ThenInclude(member => member.CharacterModel)
            .Include(group => group.Ranks).Include(group => group.Houses).FirstOrDefaultAsync(expression);
    }
}