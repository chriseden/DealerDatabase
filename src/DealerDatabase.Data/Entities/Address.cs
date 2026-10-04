

namespace DealerDatabase.Data.Entities
{
    public class Address
    {
        public int Id { get; set; }
        public int DealerId { get; set; }
        public string? AddressType { get; set; }
        public string? RawAddress { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? AddressLine3 { get; set; }
        public string? City { get; set; }
        public string? County { get; set; }
        public string? Country { get; set; }
        public string? Postcode { get; set; }
        public string? Source { get; set; }

    }
}
