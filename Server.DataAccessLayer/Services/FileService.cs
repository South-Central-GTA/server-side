using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.Core.Abstractions.ScriptStrategy;
using Server.DataAccessLayer.Context;
using Server.DataAccessLayer.Services.Base;
using Server.Database.Models.File;

namespace Server.DataAccessLayer.Services;

public class FileService
    : BaseService<FileModel>, ITransientScript
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

    public FileService(IDbContextFactory<DatabaseContext> dbContextFactory)
        : base(dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<FileModel?> GetByKey(int id)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Files
                              .Include(file => file.DirectoryModel)
                              .FirstOrDefaultAsync(file => file.Id == id);

    }
}