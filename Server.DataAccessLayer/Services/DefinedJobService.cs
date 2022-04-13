using Microsoft.EntityFrameworkCore;
using Server.Core.Abstractions.ScriptStrategy;
using Server.DataAccessLayer.Context;
using Server.DataAccessLayer.Services.Base;
using Server.Database.Models.Character;

namespace Server.DataAccessLayer.Services;

public class DefinedJobService
    : BaseService<DefinedJobModel>, ITransientScript
{
    public DefinedJobService(IDbContextFactory<DatabaseContext> dbContextFactory)
        : base(dbContextFactory)
    {
    }
}