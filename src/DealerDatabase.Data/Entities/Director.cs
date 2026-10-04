

namespace DealerDatabase.Data.Entities
{
    public class Director
    {
        public int Id { get; set; }
        public int DealerId { get; set; }
        public string? FullName { get; set; }
        public string? Role { get; set; }
        public string? Source { get; set; }
        public DateTime? AppointedOn { get; set; }
        public DateTime? ResignedOn { get; set; }
    }
}
