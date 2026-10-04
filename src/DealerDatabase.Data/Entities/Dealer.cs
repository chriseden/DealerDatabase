
using System.ComponentModel.DataAnnotations;

namespace DealerDatabase.Data.Entities;

public class Dealer
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string LineageTag { get; set; } = string.Empty;

     public string Name { get; set; } = string.Empty;    
    public string? CompanyNumber { get; set; }
    public DateTime? IncorporationDate { get; set; }
    public string? CompanyStatus { get; set; }

    public string? FcaReferenceNumber { get; set; }
    public string? FcaStatus { get; set; }
    public string? IcoRegistrationNumber { get; set; }
    public DateTime? IcoExpiryDate { get; set; }
    public string? SafStatus { get; set; }
    public DateTime? SafExpiryDate { get; set; }
    public string? VatNumber { get; set; }
    public string? Domain { get; set; }


    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedOn { get; set; } = DateTime.UtcNow;

    public List<TradingName> TradingNames { get; set; } = new();
    public List<Address> Addresses { get; set; } = new();
    public List<Contact> Contacts { get; set; } = new();
    public List<Director> Directors { get; set; } = new();
    public List<Source> Sources { get; set; } = new();
}