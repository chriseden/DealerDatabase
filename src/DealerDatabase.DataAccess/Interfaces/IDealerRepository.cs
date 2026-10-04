using DealerDatabase.Models;

namespace DealerDatabase.DataAccess.Interfaces
{
    public interface IDealerRepository
    {
        Task<int?> FindExistingDealerIdAsync( NormalisedRecord record);
        Task SaveBucketsToDatabaseAsync();
        List<DealerBucket> FillActiveBuckets(List<DealerBucket> newBuckets);
    }
}