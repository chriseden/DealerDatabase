

namespace DealerDatabase.Data.Entities
{
    public class TradingName
    {
        public int Id { get; set;  }
        public int DealerId { get; set; }
        public string? Name { get; set; }
        public string? Source { get; set; }
    }
}
