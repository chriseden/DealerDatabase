using DealerDatabase.Models;

namespace DealerDatabase.Services.Interfaces
{
    public interface IIngestionService
    {
        IReadOnlyList<DealerBucket> ActiveBuckets { get; }

        Task ProcessRecordAsync(NormalisedRecord incoming);
    }
}