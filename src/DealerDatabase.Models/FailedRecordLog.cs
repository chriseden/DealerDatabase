namespace DealerDatabase.Models
{
    public class FailedRecordLog
    {
        public int? AttemptedDealerId { get; set; }
        public string LineageTag { get; set; } = string.Empty;
        public string FailureReason { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
