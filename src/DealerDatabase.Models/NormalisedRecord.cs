using DealerDatabase.Data.Entities;

namespace DealerDatabase.Models
{ 
    public class NormalisedRecord
    {
        public string? SourceKey { get; set; }
        public string? Name { get; set; }
        public string? CompanyNumber { get; set; }
        public string? FcaNumber { get; set; }
        public string? VatNumber { get; set; }
        public string? Postcode { get; set; }
        public string? Domain { get; set; }


        public DateTime? IncorporationDate { get; set; }
        public string? CompanyStatus { get; set; }
        public string? FcaStatus { get; set; }
        public string? IcoRegistrationNumber { get; set; }
        public DateTime? IcoExpiryDate { get; set; }
        public string? SafStatus { get; set; }
        public DateTime? SafExpiryDate { get; set; }

        public List<TradingName> TradingNames { get; set; } = new();
        public List<Address> Addresses { get; set; } = new();
        public List<Contact> Contacts { get; set; } = new();
        public List<Director> Directors { get; set; } = new();

        public string? RawPayload { get; set; }
    }
}
