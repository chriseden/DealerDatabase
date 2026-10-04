using System.Text.Json.Serialization;

namespace DealerDatabase.Models.Schemas.FcaRegister
{

    public class FR_Schema
    {
        [JsonPropertyName("Message")]
        public string? Message { get; set; }
        [JsonPropertyName("ResultInfo")]
        public Resultinfo? ResultInfo { get; set; }
        [JsonPropertyName("Data")]
        public List<CompanyData> Data { get; set; } = new();
    }



    public class Resultinfo
    {
        [JsonPropertyName("total_count")]
        public int TotalCount { get; set; }
        [JsonPropertyName("exported")]
        public string? Exported { get; set; }
    }

    public class CompanyData
    {
        [JsonPropertyName("FRN")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int Frn { get; set; }
        [JsonPropertyName("Organisation Name")]
        public string? OrganisationName { get; set; }
        [JsonPropertyName("Status")]
        public string? Status { get; set; }
        [JsonPropertyName("Status Effective Date")]
        public string? StatusEffectiveDate { get; set; }
        [JsonPropertyName("Business Type")]
        public string? BusinessType { get; set; }
        [JsonPropertyName("Companies House Number")]
        public string? CompaniesHouseNumber { get; set; }
        [JsonPropertyName("Trading Names")]
        public List<string> TradingNames { get; set; } = new();
        [JsonPropertyName("Address")]
        public Address? Address { get; set; }
        [JsonPropertyName("Permissions")]
        public List<string> Permissions { get; set; } = new();
        [JsonPropertyName("Principal Firm")]
        public PrincipalFirm? PrincipalFirm { get; set; }
    }



    public class Address
    {
        [JsonPropertyName("Address Line 1")]
        public string? AddressLine1 { get; set; }
        [JsonPropertyName("Address Line 2")]
        public string? AddressLine2 { get; set; }
        [JsonPropertyName("Town")]
        public string? Town { get; set; }
        [JsonPropertyName("Postcode")]
        public string? Postcode { get; set; }
    }

    public class PrincipalFirm
    {
        [JsonPropertyName("FRN")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int? Frn { get; set; }
        [JsonPropertyName("Name")]
        public string? Name { get; set; }
    }
}