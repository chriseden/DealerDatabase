namespace DealerDatabase.Models
{
    public class OptionsMatching
    {
        public const string SectionName = "MatchingOptions";

        public double DedupeConfidenceFloor { get; set; } = 0.85;
    }
}
