using Microsoft.EntityFrameworkCore;
using Server.Core.Abstractions.ScriptStrategy;
using Server.DataAccessLayer.Context;
using Server.DataAccessLayer.Services.Base;
using Server.Database.Models.Mdc;

namespace Server.DataAccessLayer.Services;

public class MdcNoteService : BaseService<MdcNoteModel>, ITransientScript
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

    public MdcNoteService(IDbContextFactory<DatabaseContext> dbContextFactory) : base(dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }
}