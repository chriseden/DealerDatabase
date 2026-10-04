
namespace DealerDatabase.Models
{
    public class ProgressReport
    {
        public int FileIndex { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public int TotalFiles { get; set; }
        public int TotalRecordsProcessed { get; set; }
        public string StatusMessage { get; set; } = string.Empty;

        public double Percentage => TotalFiles > 0
            ? Math.Round((double)FileIndex / TotalFiles * 100, 2)
            : 0;
    }
}
