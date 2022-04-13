using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.Core.Abstractions.ScriptStrategy;
using Server.DataAccessLayer.Context;
using Server.DataAccessLayer.Services.Base;
using Server.Database.Models;

namespace Server.DataAccessLayer.Services;

public class AccountService
    : BaseService<AccountModel>, ITransientScript
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

    public AccountService(
        IDbContextFactory<DatabaseContext> dbContextFactory) : base(dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<AccountModel?> GetByName(string accountName)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Accounts
                              .FirstOrDefaultAsync(account => account.CurrentName == accountName);
    }
}