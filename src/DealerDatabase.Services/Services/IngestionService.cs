using DealerDatabase.DataAccess.Interfaces;
using DealerDatabase.Models;
using DealerDatabase.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace DealerDatabase.Services.Services
{
    public class IngestionService : IIngestionService
    {
        private readonly IMatchingService _matchingService;
        private readonly IDealerRepository _dealerRepository;
        private readonly OptionsMatching _options;
        private readonly Dictionary<string, DealerBucket> _sourceKeyToBucketMap = new();
        private readonly List<DealerBucket> _activeBuckets = new();

        public IReadOnlyList<DealerBucket> ActiveBuckets => _activeBuckets;

        public IngestionService(IMatchingService matchingService,
                                IDealerRepository dealerRepository,
                                IOptions<OptionsMatching> options)
        {
            _matchingService = matchingService;
            _dealerRepository = dealerRepository;
            _options = options.Value;
        }

        public async Task ProcessRecordAsync(NormalisedRecord incoming)
        {
            if (incoming == null || string.IsNullOrWhiteSpace(incoming.SourceKey))
            {
                return;
            }

            // 1. Check if we've already mapped this exact source record in memory
            if (_sourceKeyToBucketMap.TryGetValue(incoming.SourceKey, out var mappedBucket))
            {
                mappedBucket.AddOrUpdateRecord(incoming);
                await CheckAndUpdateExistingDealerIdAsync(mappedBucket, incoming);
                return;
            }

            // 2. Query database to check if this incoming record matches a known DB Dealer
            int? dbDealerId = await _dealerRepository.FindExistingDealerIdAsync(incoming);

            // 3. Direct route: If matched to a DB entity, check if an active bucket already exists for this DB Dealer
            if (dbDealerId.HasValue)
            {
                var bucketForDbDealer = _activeBuckets.FirstOrDefault(b => b.ExistingDealerId == dbDealerId.Value);
                if (bucketForDbDealer != null)
                {
                    bucketForDbDealer.AddOrUpdateRecord(incoming);
                    _sourceKeyToBucketMap[incoming.SourceKey] = bucketForDbDealer;
                    return;
                }
            }

            // 4. In-Memory Fuzzy Search across all active buckets
            DealerBucket? bestMatchBucket = null;
            double highestScore = 0.0;

            foreach (var bucket in _activeBuckets)
            {
                double score = _matchingService.CalculateMatchScore(incoming, bucket.RepresentativeRecord);
                if (score > highestScore)
                {
                    highestScore = score;
                    bestMatchBucket = bucket;
                }
            }

            // 5. Merge into existing fuzzy bucket or create a new one
            if (highestScore >= _options.DedupeConfidenceFloor && bestMatchBucket != null)
            {
                bestMatchBucket.AddOrUpdateRecord(incoming);

                if (dbDealerId.HasValue && !bestMatchBucket.ExistingDealerId.HasValue)
                {
                    bestMatchBucket.ExistingDealerId = dbDealerId;
                }

                _sourceKeyToBucketMap[incoming.SourceKey] = bestMatchBucket;
            }
            else
            {
                // Standalone cluster: Create a new bucket with the DB ID (if present)
                var newBucket = new DealerBucket(incoming)
                {
                    ExistingDealerId = dbDealerId
                };

                _activeBuckets.Add(newBucket);
                _sourceKeyToBucketMap[incoming.SourceKey] = newBucket;
            }
        }

        private async Task CheckAndUpdateExistingDealerIdAsync(DealerBucket bucket, NormalisedRecord incoming)
        {
            if (!bucket.ExistingDealerId.HasValue)
            {
                var dbDealerId = await _dealerRepository.FindExistingDealerIdAsync(incoming);
                if (dbDealerId.HasValue)
                {
                    bucket.ExistingDealerId = dbDealerId;
                }
            }
        }
    }
}