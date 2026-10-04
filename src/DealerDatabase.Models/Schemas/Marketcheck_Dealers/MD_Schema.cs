
using CsvHelper.Configuration.Attributes;

namespace DealerDatabase.Models.Schemas.Marketcheck_Dealers
{
    public class MD_Schema
    {
        [Name("mc_dealer_id")]
        public string? McDealerId { get; set; }

        [Name("seller_name")]
        public string? SellerName { get; set; }

        [Name("seller_type")]
        public string? SellerType { get; set; }

        [Name("franchise_make")]
        public string? FranchiseMake { get; set; }

        [Name("street")]
        public string? Street { get; set; }

        [Name("city")]
        public string? City { get; set; }

        [Name("county")]
        public string? County { get; set; }

        [Name("postcode")]
        public string? Postcode { get; set; }

        [Name("phone")]
        public string? Phone { get; set; }

        [Name("website")]
        public string? Website { get; set; }

        [Name("email")]
        public string? Email { get; set; }

        [Name("inventory_count")]
        public int? InventoryCount { get; set; }

        [Name("avg_listed_price")]
        public decimal? AvgListedPrice { get; set; }

        [Name("avg_sold_price")]
        public decimal? AvgSoldPrice { get; set; }

        [Name("avg_days_in_stock")]
        public int? AvgDaysInStock { get; set; }

        [Name("sold_last_30_days")]
        public int? SoldLast30Days { get; set; }

        [Name("vehicle_types")]
        public string? VehicleTypes { get; set; }

        [Name("car_pct")]
        public double? CarPct { get; set; }

        [Name("van_pct")]
        public double? VanPct { get; set; }

        [Name("cnt_under_80k_miles_under_7yrs")]
        public int? CntUnder80kMilesUnder7Yrs { get; set; }

        [Name("cnt_under_100k_miles_under_10yrs")]
        public int? CntUnder100kMilesUnder10Yrs { get; set; }

        [Name("cnt_under_120k_miles_under_12yrs")]
        public int? CntUnder120kMilesUnder12Yrs { get; set; }

        [Name("cnt_over_20yrs_over_50k_miles")]
        public int? CntOver20YrsOver50kMiles { get; set; }

        [Name("cnt_price_over_10k")]
        public int? CntPriceOver10k { get; set; }

        [Name("cnt_price_over_20k")]
        public int? CntPriceOver20k { get; set; }

        [Name("cnt_price_over_50k")]
        public int? CntPriceOver50k { get; set; }

        [Name("stock_feed_provider")]
        public string? StockFeedProvider { get; set; }

        [Name("last_seen")]
        [Format("yyyy-MM-dd")]
        public DateTime? LastSeen { get; set; }
    }
}

