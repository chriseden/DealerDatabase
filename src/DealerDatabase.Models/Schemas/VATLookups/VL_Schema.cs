using System.Text.Json.Serialization;

namespace DealerDatabase.Models.Schemas.VATLookups
{
    public class VL_Schema
    {
        [JsonPropertyName("target")]
        public Target? Target { get; set; }

        [JsonPropertyName("processingDate")]
        public DateTime ProcessingDate { get; set; }
    }

    public class Target
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("vatNumber")]
        public string? VatNumber { get; set; }

        [JsonPropertyName("address")]
        public Address? Address { get; set; }
    }

    public class Address
    {
        [JsonPropertyName("line1")]
        public string? Line1 { get; set; }

        [JsonPropertyName("line2")]
        public string? Line2 { get; set; }

        [JsonPropertyName("line3")]
        public string? Line3 { get; set; }

        [JsonPropertyName("postcode")]
        public string? Postcode { get; set; }

        [JsonPropertyName("countryCode")]
        public string? CountryCode { get; set; }
    }
}