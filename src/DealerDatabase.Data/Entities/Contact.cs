

namespace DealerDatabase.Data.Entities
{
    public class Contact
    {
        public int Id { get; set; }
        public int DealerId { get; set; }
        public string? ContactType { get; set; }
        public string? Value { get; set; }
        public string? Source { get; set; }
    }
}
