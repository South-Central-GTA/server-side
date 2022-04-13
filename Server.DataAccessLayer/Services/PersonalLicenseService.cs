using Microsoft.EntityFrameworkCore;
using Server.Core.Abstractions.ScriptStrategy;
using Server.DataAccessLayer.Context;
using Server.DataAccessLayer.Services.Base;
using Server.Database.Enums;
using Server.Database.Models;
using Server.Database.Models.Character;
using Server.Database.Models.Mdc;

namespace Server.DataAccessLayer.Services;

public class PersonalLicenseService
    : BaseService<PersonalLicenseModel>, ITransientScript
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

    public PersonalLicenseService(IDbContextFactory<DatabaseContext> dbContextFactory)
        : base(dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }
}