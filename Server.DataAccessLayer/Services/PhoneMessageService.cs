using Microsoft.EntityFrameworkCore;
using Server.Core.Abstractions.ScriptStrategy;
using Server.DataAccessLayer.Context;
using Server.DataAccessLayer.Services.Base;
using Server.Database.Models.Inventory.Phone;

namespace Server.DataAccessLayer.Services;

public class PhoneMessageService : BaseService<PhoneMessageModel>, ITransientScript
{
    public PhoneMessageService(IDbContextFactory<DatabaseContext> dbContextFactory) : base(dbContextFactory)
    {
    }
}