namespace foodstreet_admin.Models;

public class AdminStatsModel
{
    public int TotalStores { get; set; }
    public int PendingStores { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalListens { get; set; }
    public decimal TotalRevenue { get; set; }

    public List<TopStoreModel> TopStores { get; set; } = new();
    public List<LangStatModel> LangStats { get; set; } = new();
    public List<MonthlyOverviewModel> MonthlyOverview { get; set; } = new();
}

public class TopStoreModel
{
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public int Listens { get; set; }
}

public class LangStatModel
{
    public string Language { get; set; } = "";
    public int Listens { get; set; }
    public double Percent { get; set; }
}

public class MonthlyOverviewModel
{
    public string Month { get; set; } = "";
    public int NewStores { get; set; }
    public int NewCustomers { get; set; }
    public int TotalListens { get; set; }
    public decimal Revenue { get; set; }
    public double Growth { get; set; }
}

// ── Admin Revenue Models ──────────────────────────────────────────
public class AdminRevenueModel
{
    public int Year { get; set; }
    public int? Month { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<ShopRevenueItem> RevenueByShop { get; set; } = new();
    public List<MonthlyRevenueItem> RevenueByMonth { get; set; } = new();
}

public class ShopRevenueItem
{
    public int ShopId { get; set; }
    public string ShopName { get; set; } = "";
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}

public class MonthlyRevenueItem
{
    public int Month { get; set; }
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}
