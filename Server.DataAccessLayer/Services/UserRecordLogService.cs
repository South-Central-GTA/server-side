using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.Core.Abstractions.ScriptStrategy;
using Server.DataAccessLayer.Context;
using Server.DataAccessLayer.Services.Base;
using Server.Database.Models.CustomLogs;

namespace Server.DataAccessLayer.Services;

public class UserRecordLogService : BaseService<UserRecordLogModel>, ITransientScript
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

    public UserRecordLogService(IDbContextFactory<DatabaseContext> dbContextFactory) : base(dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public override async Task<List<UserRecordLogModel>> Where(Expression<Func<UserRecordLogModel, bool>> expression)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.UserRecordLogs.Include(d => d.AccountModel).Include(d => d.StaffAccountModel)
            .Include(d => d.CharacterModel).Where(expression).ToListAsync();
    }
}