using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.Core.Abstractions.ScriptStrategy;
using Server.DataAccessLayer.Context;
using Server.DataAccessLayer.Services.Base;
using Server.Database.Models;
using Server.Database.Models.CustomLogs;

namespace Server.DataAccessLayer.Services;

public class RegistrationOfficeService
    : BaseService<RegistrationOfficeEntryModel>, ITransientScript
{
    public RegistrationOfficeService(IDbContextFactory<DatabaseContext> dbContextFactory)
        : base(dbContextFactory)
    {
    }

    public async Task<bool> IsRegistered(int characterModelId)
    {
        return await GetByKey(characterModelId) != null;
    }
}