using Microsoft.EntityFrameworkCore;
using Server.Core.Abstractions.ScriptStrategy;
using Server.DataAccessLayer.Context;
using Server.DataAccessLayer.Services.Base;
using Server.Database.Models.Inventory.Phone;

namespace Server.DataAccessLayer.Services;

public class PhoneContactService
    : BaseService<PhoneContactModel>, ITransientScript
{
    public PhoneContactService(IDbContextFactory<DatabaseContext> dbContextFactory)
        : base(dbContextFactory)
    {
    }
}