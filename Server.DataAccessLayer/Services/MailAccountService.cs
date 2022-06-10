using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.Core.Abstractions.ScriptStrategy;
using Server.DataAccessLayer.Context;
using Server.DataAccessLayer.Services.Base;
using Server.Database.Models.Mail;

namespace Server.DataAccessLayer.Services;

public class MailAccountService : BaseService<MailAccountModel>, ITransientScript
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

    public MailAccountService(IDbContextFactory<DatabaseContext> dbContextFactory) : base(dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<MailAccountModel?> GetByKey(string mailAddress)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.MailAccounts.Include(m => m.MailLinks).ThenInclude(m => m.MailModel)
            .Include(m => m.GroupAccess).ThenInclude(ga => ga.GroupModel).Include(m => m.CharacterAccesses)
            .ThenInclude(access => access.CharacterModel).FirstOrDefaultAsync(m => m.MailAddress == mailAddress);
    }

    public override async Task<List<MailAccountModel>> Where(Expression<Func<MailAccountModel, bool>> expression)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.MailAccounts.Include(m => m.GroupAccess).Include(m => m.CharacterAccesses)
            .ThenInclude(access => access.CharacterModel).Where(expression).ToListAsync();
    }

    public override async Task<List<MailAccountModel>> GetAll()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.MailAccounts.Include(m => m.MailLinks).ThenInclude(m => m.MailModel)
            .Include(m => m.GroupAccess).ThenInclude(ga => ga.GroupModel).Include(bank => bank.CharacterAccesses)
            .ThenInclude(ca => ca.CharacterModel).ToListAsync();
    }

    public async Task<List<MailAccountModel>> GetByCharacter(int characterId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.MailAccounts.Include(m => m.MailLinks).ThenInclude(m => m.MailModel)
            .Include(m => m.GroupAccess).Include(bank => bank.CharacterAccesses).ThenInclude(ca => ca.CharacterModel)
            .Where(m => m.CharacterAccesses != null && m.CharacterAccesses.Any(m => m.CharacterModelId == characterId))
            .ToListAsync();
    }

    public async Task<List<MailAccountModel>> GetByOwner(int characterId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.MailAccounts.Include(m => m.MailLinks).ThenInclude(m => m.MailModel)
            .Include(m => m.GroupAccess).Include(m => m.CharacterAccesses).ThenInclude(ca => ca.CharacterModel)
            .Where(m => m.CharacterAccesses != null &&
                        m.CharacterAccesses.Any(m => m.CharacterModelId == characterId && m.Owner)).ToListAsync();
    }

    public async Task<MailAccountModel?> GetByGroup(int groupId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.MailAccounts.Include(m => m.MailLinks).ThenInclude(m => m.MailModel)
            .Include(m => m.GroupAccess).Include(m => m.CharacterAccesses).ThenInclude(ca => ca.CharacterModel)
            .FirstOrDefaultAsync(m => m.GroupAccess.Any(ga => ga.GroupModelId == groupId));
    }

    public async Task<MailAccountModel?> GetByOwningGroup(int groupId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.MailAccounts.Include(m => m.MailLinks).ThenInclude(m => m.MailModel)
            .Include(m => m.GroupAccess).Include(m => m.CharacterAccesses).ThenInclude(ca => ca.CharacterModel)
            .FirstOrDefaultAsync(m => m.GroupAccess.Any(ga => ga.GroupModelId == groupId && ga.Owner));
    }
}