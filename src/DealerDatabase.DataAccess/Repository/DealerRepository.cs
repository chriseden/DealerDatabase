using DealerDatabase.Data;
using DealerDatabase.Data.Entities;
using DealerDatabase.DataAccess.Interfaces;
using DealerDatabase.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DealerDatabase.DataAccess.Repository
{
    public class DealerRepository : IDealerRepository
    {
        private readonly DealerDbContext _context;
        private readonly TimeProvider _timeProvider;
        private readonly ILogger<DealerRepository> _logger;
        public List<DealerBucket> _activeBuckets = new List<DealerBucket>();
        private readonly List<FailedRecordLog> failedRecords = new();

        public DealerRepository(
            DealerDbContext dbContext,
            TimeProvider timeProvider,
            ILogger<DealerRepository> logger)
        {
            _context = dbContext;
            _timeProvider = timeProvider;
            _logger = logger;
        }

        public async Task<int?> FindExistingDealerIdAsync(NormalisedRecord record)
        {
            // Check Hard Regulatory Identifiers
            if (!string.IsNullOrEmpty(record.CompanyNumber) ||
                !string.IsNullOrEmpty(record.FcaNumber) ||
                !string.IsNullOrEmpty(record.VatNumber))
            {
                var match = await _context.Dealers
                    .Select(d => new { d.Id, d.CompanyNumber, d.FcaReferenceNumber, d.VatNumber })
                    .FirstOrDefaultAsync(d =>
                        (!string.IsNullOrEmpty(record.CompanyNumber) && d.CompanyNumber == record.CompanyNumber) ||
                        (!string.IsNullOrEmpty(record.FcaNumber) && d.FcaReferenceNumber == record.FcaNumber) ||
                        (!string.IsNullOrEmpty(record.VatNumber) && d.VatNumber == record.VatNumber)
                    );

                if (match != null) return match.Id;
            }

            // Check Domain Name as a fallback
            if (!string.IsNullOrEmpty(record.Domain))
            {
                var domainMatch = await _context.Dealers
                    .Where(d => d.Domain == record.Domain)
                    .Select(d => (int?)d.Id)
                    .FirstOrDefaultAsync();

                if (domainMatch.HasValue) return domainMatch.Value;
            }

            // Check Linked Address Postcode + Trading Name
            if (!string.IsNullOrEmpty(record.Postcode) && !string.IsNullOrEmpty(record.Name))
            {
                var cleanName = record.Name.Trim();
                var cleanPostcode = record.Postcode.Replace(" ", "").ToUpperInvariant();

                var locationMatch = await _context.Dealers
                    .Where(d => d.Addresses.Any(a =>
                        a.Postcode != null &&
                        a.Postcode.Replace(" ", "").ToUpper() == cleanPostcode))
                    .Where(d => d.TradingNames.Any(tn =>
                        tn.Name.Trim().ToLower() == cleanName.ToLower()))
                    .Select(d => (int?)d.Id)
                    .FirstOrDefaultAsync();

                if (locationMatch.HasValue) return locationMatch.Value;
            }

            return null;
        }

        public async Task SaveBucketsToDatabaseAsync()
        {
            foreach (var bucket in _activeBuckets)
            {
                Dealer dealerEntity;

                if (bucket.ExistingDealerId.HasValue)
                {
                    try
                    {
                        dealerEntity = await _context.Dealers
                            .Include(d => d.TradingNames)
                            .Include(d => d.Addresses)
                            .Include(d => d.Contacts)
                            .Include(d => d.Directors) 
                            .SingleAsync(d => d.Id == bucket.ExistingDealerId.Value);
                    }
                    catch (InvalidOperationException ex)
                    {
                        _logger.LogError(
                            ex,
                            "Failed to save Buckets for LineageTag {LineageTag} (Dealer ID: {DealerId}). Continuing with next record.",
                            bucket.BuildLineageTag(),
                            bucket.ExistingDealerId
                        );

                        failedRecords.Add(new FailedRecordLog
                        {
                            AttemptedDealerId = bucket.ExistingDealerId,
                            LineageTag = bucket.BuildLineageTag(),
                            FailureReason = ex.Message
                        });
                        continue;
                    }
                }
                else
                {
                    dealerEntity = new Dealer
                    {
                        TradingNames = new List<TradingName>(),
                        Addresses = new List<Data.Entities.Address>(),
                        Directors = new List<Director>(),
                        Contacts = new List<Contact>()
                    };
                    _context.Dealers.Add(dealerEntity);
                }

                dealerEntity.LineageTag = bucket.BuildLineageTag();
                dealerEntity.Name = bucket.ResolveBestName();
                dealerEntity.CompanyNumber = bucket.RepresentativeRecord.CompanyNumber;
                dealerEntity.FcaReferenceNumber = bucket.RepresentativeRecord.FcaNumber;
                dealerEntity.CompanyStatus = bucket.RepresentativeRecord.CompanyStatus;
                dealerEntity.FcaStatus = bucket.RepresentativeRecord.FcaStatus;
                dealerEntity.VatNumber = bucket.RepresentativeRecord.VatNumber;
                dealerEntity.IcoRegistrationNumber = bucket.RepresentativeRecord.IcoRegistrationNumber;
                dealerEntity.IncorporationDate = bucket.RepresentativeRecord.IncorporationDate;
                dealerEntity.IcoExpiryDate = bucket.RepresentativeRecord.IcoExpiryDate;
                dealerEntity.SafStatus = bucket.RepresentativeRecord.SafStatus;
                dealerEntity.SafExpiryDate = bucket.RepresentativeRecord.SafExpiryDate;
                dealerEntity.Domain = bucket.RepresentativeRecord.Domain;
                dealerEntity.ModifiedOn = _timeProvider.GetUtcNow().UtcDateTime;

                SyncChildCollections(dealerEntity, bucket);
            }

            await _context.SaveChangesAsync();
            await RecordFailedRecords();
        }

        public List<DealerBucket> FillActiveBuckets(List<DealerBucket> newBuckets)
        {
            if (newBuckets is not null && newBuckets.Any())
            {
                _activeBuckets = newBuckets;
            }
            return _activeBuckets;
        }

        private async Task RecordFailedRecords()
        {
            if (failedRecords.Any())
            {
                string logPath = SolutionPaths.GetFailedRecordsFilePath();
                string jsonContent = System.Text.Json.JsonSerializer.Serialize(failedRecords, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

                await File.WriteAllTextAsync(logPath, jsonContent);

                _logger.LogWarning("{Count} record(s) failed during processing. Details written to {LogPath}", failedRecords.Count, logPath);
            }
        }

        private void SyncChildCollections(Dealer dealer, DealerBucket bucket)
        {
            foreach (var record in bucket.MemberRecords)
            {
                if (!string.IsNullOrWhiteSpace(record.Name))
                {
                    var cleanName = record.Name.Trim();
                    if (!dealer.TradingNames.Any(tn => tn.Name.Equals(cleanName, StringComparison.OrdinalIgnoreCase)))
                    {
                        dealer.TradingNames.Add(new TradingName
                        {
                            Name = cleanName,
                            Source = record.SourceKey
                        });
                    }
                }

                if (record.TradingNames != null)
                {
                    foreach (var tn in record.TradingNames)
                    {
                        if (!string.IsNullOrWhiteSpace(tn.Name))
                        {
                            var cleanName = tn.Name.Trim();
                            if (!dealer.TradingNames.Any(existing => existing.Name.Equals(cleanName, StringComparison.OrdinalIgnoreCase)))
                            {
                                dealer.TradingNames.Add(new TradingName
                                {
                                    Name = cleanName,
                                    Source = record.SourceKey
                                });
                            }
                        }
                    }
                }

                if (record.Addresses != null)
                {
                    foreach (var addr in record.Addresses)
                    {
                        var cleanLine1 = addr.AddressLine1?.Trim();
                        var cleanPostcode = addr.Postcode?.Replace(" ", "").ToUpperInvariant();

                        if (!string.IsNullOrWhiteSpace(cleanLine1) || !string.IsNullOrWhiteSpace(cleanPostcode))
                        {
                            bool addressExists = dealer.Addresses.Any(a =>
                                (a.Postcode != null && a.Postcode.Replace(" ", "").ToUpperInvariant() == cleanPostcode) &&
                                (a.AddressLine1 != null && a.AddressLine1.Trim().Equals(cleanLine1, StringComparison.OrdinalIgnoreCase)));

                            if (!addressExists)
                            {
                                dealer.Addresses.Add(new Data.Entities.Address
                                {
                                    AddressLine1 = addr.AddressLine1?.Trim(),
                                    AddressLine2 = addr.AddressLine2?.Trim(),
                                    AddressLine3 = addr.AddressLine3?.Trim(),
                                    AddressType = addr.AddressType?.ToUpperInvariant(),
                                    City = addr.City?.Trim(),
                                    County = addr.County?.Trim(),
                                    Postcode = addr.Postcode?.Trim(),
                                    Country = addr.Country?.Trim(),
                                    Source = record.SourceKey
                                });
                            }
                        }
                    }
                }

                if (record.Directors != null)
                {
                    foreach (var director in record.Directors)
                    {
                        if (!string.IsNullOrWhiteSpace(director.FullName))
                        {
                            var cleanDirectorName = director.FullName.Trim();
                            bool directorExists = dealer.Directors.Any(o =>
                                o.FullName.Equals(cleanDirectorName, StringComparison.OrdinalIgnoreCase) &&
                                o.Role == director.Role);

                            if (!directorExists)
                            {
                                dealer.Directors.Add(new Director
                                {
                                    FullName = cleanDirectorName,
                                    Role = director.Role,
                                    AppointedOn = director.AppointedOn,
                                    ResignedOn = director.ResignedOn,
                                    Source = record.SourceKey
                                });
                            }
                        }
                    }
                }

                 if (record.Contacts != null)
                {
                    foreach (var contact in record.Contacts)
                    {
                        var cleanValue = contact.Value?.Trim();
                        if (!string.IsNullOrWhiteSpace(cleanValue))
                        {
                            bool contactExists = dealer.Contacts.Any(c =>
                                c.ContactType == contact.ContactType &&
                                c.Value.Equals(cleanValue, StringComparison.OrdinalIgnoreCase));

                            if (!contactExists)
                            {
                                dealer.Contacts.Add(new Contact
                                {
                                    ContactType = contact.ContactType,
                                    Value = cleanValue,
                                    Source = record.SourceKey
                                });
                            }
                        }
                    }
                }
            }
        }
    }
}