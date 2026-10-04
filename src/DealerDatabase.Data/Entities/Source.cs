

namespace DealerDatabase.Data.Entities
{
    public class Source
    {
        public int Id { get; set; }
        public int DealerId { get; set; }
        public string? SourceName { get; set; } 
        public string? RawPayloadJson { get; set; }
    }
}
