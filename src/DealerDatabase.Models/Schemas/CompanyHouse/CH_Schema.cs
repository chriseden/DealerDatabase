using System.Text.Json.Serialization;

namespace DealerDatabase.Models.Schemas.CompanyHouse
{
    public class CH_Schema
    {
        [JsonPropertyName("source")]
        public string? Source { get; set; }

        [JsonPropertyName("extracted_at")]
        public DateTime ExtractedAt { get; set; }

        [JsonPropertyName("items_count")]
        public int? ItemsCount { get; set; }

        [JsonPropertyName("items")]
        public List<Item> Items { get; set; } = new();
    }

    public class Item
    {
        [JsonPropertyName("company_number")]
        public string? CompanyNumber { get; set; }

        [JsonPropertyName("company_name")]
        public string? CompanyName { get; set; }

        [JsonPropertyName("company_status")]
        public string? CompanyStatus { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("date_of_creation")]
        public string? DateOfCreation { get; set; }

        [JsonPropertyName("registered_office_address")]
        public RegisteredOfficeAddress? RegisteredOfficeAddress { get; set; }

        [JsonPropertyName("sic_codes")]
        public List<string> SicCodes { get; set; } = new();

        [JsonPropertyName("officers")]
        public List<Officer> Officers { get; set; } = new();

        [JsonPropertyName("date_of_cessation")]
        public string? DateOfCessation { get; set; }

        [JsonPropertyName("previous_company_names")]
        public List<PreviousCompanyName> PreviousCompanyNames { get; set; } = new();
    }

    public class RegisteredOfficeAddress
    {
        [JsonPropertyName("address_line_1")]
        public string? AddressLine1 { get; set; }

        [JsonPropertyName("address_line_2")]
        public string? AddressLine2 { get; set; }

        [JsonPropertyName("locality")]
        public string? Locality { get; set; }

        [JsonPropertyName("region")]
        public string? Region { get; set; }

        [JsonPropertyName("postal_code")]
        public string? PostalCode { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }
    }

    public class Officer
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("officer_role")]
        public string? OfficerRole { get; set; }

        [JsonPropertyName("appointed_on")]
        public string? AppointedOn { get; set; }

        [JsonPropertyName("occupation")]
        public string? Occupation { get; set; }

        [JsonPropertyName("nationality")]
        public string? Nationality { get; set; }

        [JsonPropertyName("resigned_on")]
        public string? ResignedOn { get; set; }
    }

    public class PreviousCompanyName
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("effective_from")]
        public string? EffectiveFrom { get; set; }

        [JsonPropertyName("ceased_on")]
        public string? CeasedOn { get; set; }
    }
}