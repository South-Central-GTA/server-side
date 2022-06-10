using Microsoft.EntityFrameworkCore;
using Server.Core.Abstractions.ScriptStrategy;
using Server.DataAccessLayer.Context;
using Server.DataAccessLayer.Services.Base;
using Server.Database.Models.Banking;

namespace Server.DataAccessLayer.Services;

public class BankAccountCharacterAccessService : BaseService<BankAccountCharacterAccessModel>, ITransientScript
{
    public BankAccountCharacterAccessService(IDbContextFactory<DatabaseContext> dbContextFactory) : base(
        dbContextFactory)
    {
    }
}