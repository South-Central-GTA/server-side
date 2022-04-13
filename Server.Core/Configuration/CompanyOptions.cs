using System.Collections.Generic;
using Server.Data.Models;
using Server.Database.Enums;

namespace Server.Core.Configuration;

public class CompanyOptions
{
    public int CreateCosts { get; set; }
    public int CashierPayDayCosts { get; set; }
    public Dictionary<LeaseCompanyType, int> RevenueEachPayday { get; set; }
    public Dictionary<LeaseCompanyType, CompanyData> Types { get; set; }
    public int MaxLicenses { get; set; }
    public int MaxProducts { get; set; }
    public List<LicensesData> Licenses { get; set; }

    public struct CompanyData
    {
        public string Name { get; set; }
        public string Cashier { get; set; }
    }
}