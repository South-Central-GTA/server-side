using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.Core.Abstractions.ScriptStrategy;
using Server.DataAccessLayer.Context;
using Server.DataAccessLayer.Services.Base;
using Server.Database.Models.File;

namespace Server.DataAccessLayer.Services;

public class DirectoryService
    : BaseService<DirectoryModel>, ITransientScript
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

    public DirectoryService(IDbContextFactory<DatabaseContext> dbContextFactory)
        : base(dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<DirectoryModel?> GetByKey(int id)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Directories
                              .Include(directory => directory.GroupModel)
                              .ThenInclude(group => group.Members)
                              .FirstOrDefaultAsync(directory => directory.Id == id);
    }

    public override async Task<DirectoryModel?> Find(Expression<Func<DirectoryModel, bool>> expression)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Directories
                              .Include(directory => directory.Files)
                              .FirstOrDefaultAsync(expression);
    }

    public override async Task<List<DirectoryModel>> Where(Expression<Func<DirectoryModel, bool>> expression)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Directories
                              .Include(directory => directory.Files)
                              .Where(expression)
                              .ToListAsync();
    }
}