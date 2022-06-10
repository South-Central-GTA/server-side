using Microsoft.EntityFrameworkCore;
using Server.Core.Abstractions.ScriptStrategy;
using Server.DataAccessLayer.Context;
using Server.DataAccessLayer.Services.Base;
using Server.Database.Models.Mdc;

namespace Server.DataAccessLayer.Services;

public class EmergencyCallService : BaseService<EmergencyCallModel>, ITransientScript
{
    public EmergencyCallService(IDbContextFactory<DatabaseContext> dbContextFactory) : base(dbContextFactory)
    {
    }
}