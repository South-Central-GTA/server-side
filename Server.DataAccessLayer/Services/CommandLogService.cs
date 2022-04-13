using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.Core.Abstractions.ScriptStrategy;
using Server.DataAccessLayer.Context;
using Server.DataAccessLayer.Services.Base;
using Server.Database.Models.CustomLogs;

namespace Server.DataAccessLayer.Services;

public class CommandLogService
    : BaseService<CommandLogModel>, ITransientScript
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

    public CommandLogService(IDbContextFactory<DatabaseContext> dbContextFactory)
        : base(dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public override async Task<List<CommandLogModel>> Where(Expression<Func<CommandLogModel, bool>> expression)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.CommandLogs
                              .Include(c => c.AccountModel)
                              .Include(c => c.CharacterModel)
                              .ToListAsync();
    }
}