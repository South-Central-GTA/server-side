using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.Core.Abstractions.ScriptStrategy;
using Server.DataAccessLayer.Context;
using Server.DataAccessLayer.Services.Base;
using Server.Database.Models.Banking;

namespace Server.DataAccessLayer.Services;

public class BankAccountService
    : BaseService<BankAccountModel>, ITransientScript
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

    public BankAccountService(IDbContextFactory<DatabaseContext> dbContextFactory)
        : base(dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<BankAccountModel?> GetByKey(int bankId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.BankAccounts
                              .Include(bank => bank.History)
                              .Include(bank => bank.GroupRankAccess)
                              .ThenInclude(access => access.GroupModel)
                              .Include(bank => bank.CharacterAccesses)
                              .ThenInclude(access => access.CharacterModel)
                              .FirstOrDefaultAsync(bank => bank.Id == bankId);
    }

    public override async Task<List<BankAccountModel>> Where(Expression<Func<BankAccountModel, bool>> expression)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.BankAccounts
                              .Include(bank => bank.History)
                              .Include(bank => bank.GroupRankAccess)
                              .ThenInclude(access => access.GroupModel)
                              .Include(bank => bank.CharacterAccesses)
                              .ThenInclude(access => access.CharacterModel)
                              .Where(expression).ToListAsync();
    }

    public override async Task<List<BankAccountModel>> GetAll()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.BankAccounts
                              .Include(bank => bank.History)
                              .Include(bank => bank.GroupRankAccess)
                              .ThenInclude(access => access.GroupModel)
                              .Include(bank => bank.CharacterAccesses)
                              .ThenInclude(access => access.CharacterModel)
                              .ToListAsync();
    }

    public override async Task<BankAccountModel?> Find(Expression<Func<BankAccountModel, bool>> expression)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.BankAccounts
                              .Include(bank => bank.History)
                              .Include(bank => bank.GroupRankAccess)
                              .ThenInclude(access => access.GroupModel)
                              .Include(bank => bank.CharacterAccesses)
                              .ThenInclude(access => access.CharacterModel)
                              .FirstOrDefaultAsync(expression);
    }

    public async Task<BankAccountModel?> GetByBankDetails(string bankAccountDetails)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.BankAccounts
                              .Include(bank => bank.History)
                              .Include(bank => bank.GroupRankAccess)
                              .ThenInclude(access => access.GroupModel)
                              .Include(bank => bank.CharacterAccesses)
                              .ThenInclude(access => access.CharacterModel)
                              .FirstOrDefaultAsync(bank => bank.BankDetails == bankAccountDetails);
    }

    public async Task<List<BankAccountModel>> GetByCharacter(int characterId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.BankAccounts
                              .Include(bank => bank.History)
                              .Include(bank => bank.GroupRankAccess)
                              .ThenInclude(access => access.GroupModel)
                              .Include(bank => bank.CharacterAccesses)
                              .ThenInclude(ca => ca.CharacterModel)
                              .Where(bank => bank.CharacterAccesses != null &&
                                             bank.CharacterAccesses.Any(m => m.CharacterModelId == characterId))
                              .ToListAsync();
    }

    public async Task<List<BankAccountModel>> GetByOwner(int characterId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.BankAccounts
                              .Include(bank => bank.History)
                              .Include(bank => bank.GroupRankAccess)
                              .ThenInclude(access => access.GroupModel)
                              .Include(bank => bank.CharacterAccesses)
                              .ThenInclude(ca => ca.CharacterModel)
                              .Where(bank => bank.CharacterAccesses != null &&
                                             bank.CharacterAccesses.Any(
                                                 m => m.CharacterModelId == characterId && m.Owner))
                              .ToListAsync();
    }

    public async Task<BankAccountModel?> GetByGroup(int groupId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.BankAccounts
                              .Include(bank => bank.History)
                              .Include(bank => bank.GroupRankAccess)
                              .ThenInclude(access => access.GroupModel)
                              .Include(bank => bank.CharacterAccesses)
                              .ThenInclude(ca => ca.CharacterModel)
                              .FirstOrDefaultAsync(bank => bank.GroupRankAccess.Any(ga => ga.GroupModelId == groupId));
    }

    public async Task<BankAccountModel?> GetByOwningGroup(int groupId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.BankAccounts
                              .Include(bank => bank.History)
                              .Include(bank => bank.GroupRankAccess)
                              .ThenInclude(access => access.GroupModel)
                              .Include(bank => bank.CharacterAccesses)
                              .ThenInclude(ca => ca.CharacterModel)
                              .FirstOrDefaultAsync(
                                  bank => bank.GroupRankAccess.Any(ga => ga.GroupModelId == groupId && ga.Owner));
    }
}