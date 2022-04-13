using System.Collections.Generic;
using Server.Data.Models;

namespace Server.Core.Configuration;

public class GameOptions
{
    public int SaveInterval { get; set; }
    public float MoneyToPointsExchangeRate { get; set; }
    public int BankMinutesInterval { get; set; }
    public int PayDayMinute { get; set; }
    public int CompanyMinutesInterval { get; set; }
    public int DeleteDroppedItemsAfterDays { get; set; }
    public int DeleteRoleplayInfosAfterDays { get; set; }
    public int BankMoneyUntilTaxes { get; set; }
    public float TaxesExchangeRate { get; set; }
    public float RepairVehiclePercentage { get; set; }
    public List<DefinedJobData> DefinedJobs { get; set; }
}