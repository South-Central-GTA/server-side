using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AltV.Net.Data;
using Microsoft.EntityFrameworkCore;
using Server.Core.Abstractions.ScriptStrategy;
using Server.DataAccessLayer.Context;
using Server.DataAccessLayer.Services.Base;
using Server.Database.Models.Group;

namespace Server.DataAccessLayer.Services;

public class CompanyGroupService : BaseService<CompanyGroupModel>, ITransientScript
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

    public CompanyGroupService(IDbContextFactory<DatabaseContext> dbContextFactory) : base(dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<CompanyGroupModel?> GetByCharacter(int characterId)
    {
        var ownedFaction = await GetByOwner(characterId);
        if (ownedFaction != null)
        {
            return ownedFaction;
        }

        var memberFaction = await GetByMember(characterId);

        return memberFaction;
    }

    public override async Task<CompanyGroupModel?> GetByKey(object id)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.CompanyGroups.Include(group => group.Members)
            .ThenInclude(member => member.CharacterModel).Include(group => group.Ranks)
            .FirstOrDefaultAsync(group => group.Id == (int)id);
    }

    public override async Task<List<CompanyGroupModel>> GetAll()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.CompanyGroups.Include(group => group.Members)
            .ThenInclude(member => member.CharacterModel).Include(group => group.Ranks).ToListAsync();
    }

    public async Task<CompanyGroupModel?> GetByOwner(int characterId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.CompanyGroups.Include(group => group.Members)
            .ThenInclude(member => member.CharacterModel).Include(group => group.Ranks).FirstOrDefaultAsync(group =>
                group.Members != null && group.Members.Any(m => m.CharacterModelId == characterId && m.Owner));
    }

    public async Task<CompanyGroupModel?> GetByMember(int characterId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.CompanyGroups.Include(group => group.Members)
            .ThenInclude(member => member.CharacterModel).Include(group => group.Ranks).FirstOrDefaultAsync(group =>
                group.Members != null && group.Members.Any(m => m.CharacterModelId == characterId && !m.Owner));
    }
    
    public async Task<CompanyGroupModel?> GetByClosestVehicleInteractionPoint(Position position, float maxDistance = 5f)
    {
        var closestDistance = float.MaxValue;
        CompanyGroupModel companyGroupModel = null;
        foreach (var companyGroup in await GetAll())
        {
            if (!companyGroup.VehicleInteractionPointX.HasValue || !companyGroup.VehicleInteractionPointY.HasValue || !companyGroup.VehicleInteractionPointZ.HasValue)
            {
                continue;
            }
            
            var distance = new Position(companyGroup.VehicleInteractionPointX.Value, 
                companyGroup.VehicleInteractionPointY.Value, 
                companyGroup.VehicleInteractionPointZ.Value).Distance(position);
            
            if (distance <= maxDistance && distance < closestDistance)
            {
                closestDistance = distance;
                companyGroupModel = companyGroup;
            }
        }

        return companyGroupModel;
    }

    public override async Task<List<CompanyGroupModel>> Where(Expression<Func<CompanyGroupModel, bool>> expression)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.CompanyGroups.Include(group => group.Members)
            .ThenInclude(member => member.CharacterModel).Include(group => group.Ranks).Where(expression).ToListAsync();
    }

    public override async Task<CompanyGroupModel?> Find(Expression<Func<CompanyGroupModel, bool>> expression)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.CompanyGroups.Include(group => group.Members)
            .ThenInclude(member => member.CharacterModel).Include(group => group.Ranks).FirstOrDefaultAsync(expression);
    }
}