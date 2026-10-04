
using System.ComponentModel;
using System.Xml.Serialization;

namespace DealerDatabase.Models.Schemas.SAFMembers
{
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public partial class SafRegister
    {
        [XmlElement("Member")]
        public List<SafRegisterMember> Members { get; set; } = new();

        [XmlAttribute("exported", DataType = "date")]
        public DateTime Exported { get; set; }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class SafRegisterMember
    {
        [XmlAttribute("id")]
        public string? Id { get; set; }

        public string? Name { get; set; }

        public string? LegalName { get; set; }

        public string? TradingAs { get; set; }

        public string? Town { get; set; }

        public string? Postcode { get; set; }

        public string? Telephone { get; set; }

        public string? Website { get; set; }

        public string? Status { get; set; }

        public string? Expiry { get; set; }
    }
}