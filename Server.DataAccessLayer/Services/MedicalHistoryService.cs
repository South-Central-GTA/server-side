using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.Core.Abstractions.ScriptStrategy;
using Server.DataAccessLayer.Context;
using Server.DataAccessLayer.Services.Base;
using Server.Database.Models.Mdc;

namespace Server.DataAccessLayer.Services;

public class MedicalHistoryService
    : BaseService<MdcMedicalEntryModel>, ITransientScript
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

    public MedicalHistoryService(IDbContextFactory<DatabaseContext> dbContextFactory)
        : base(dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }
    
    public async Task<MdcMedicalEntryModel?> GetByKey(int id)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.MdcMedicalEntries
                              .Include(i => i.CharacterModel)
                              .FirstOrDefaultAsync(i => i.Id == id);
    }
    
    public override async Task<List<MdcMedicalEntryModel>> Where(Expression<Func<MdcMedicalEntryModel, bool>> expression)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.MdcMedicalEntries
                              .Include(i => i.CharacterModel)
                              .Where(expression).ToListAsync();
    }
}