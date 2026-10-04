using CsvHelper;
using CsvHelper.Configuration;
using DealerDatabase.Data.Entities;
using DealerDatabase.Models;
using DealerDatabase.Models.Constants;
using DealerDatabase.Models.Schemas.Marketcheck_Dealers;
using DealerDatabase.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace DealerDatabase.Services.Readers
{
    public class MarketcheckReader : IFileDataReader
    {
        private readonly ILogger<MarketcheckReader> _logger;

        public MarketcheckReader(ILogger<MarketcheckReader> logger)
        {
            _logger = logger;
        }

        public IEnumerable<NormalisedRecord> ReadFile(string filePath)
        {
            _logger.LogDebug("Reading Marketcheck CSV file {filePath}", filePath);

            if (!File.Exists(filePath))
            {
                _logger.LogError("File not found: {filePath}", filePath);
                yield break;
            }

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null,
                PrepareHeaderForMatch = args => args.Header.ToLowerInvariant().Trim()
            };

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, config);

            if (!csv.Read() || !csv.ReadHeader())
            {
                _logger.LogWarning("CSV file is empty or missing headers: {filePath}", filePath);
                yield break;
            }

            int processedCount = 0;

            while (csv.Read())
            {
                string rawLine = csv.Parser.RawRecord;

                MD_Schema? record;
                try
                {
                    record = csv.GetRecord<MD_Schema>();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse CSV row in {filePath}. Skipping row.", filePath);
                    continue;
                }

                if (record == null) continue;

                var normalisedRecord = BuildNormalisedRecord(record);
                normalisedRecord.RawPayload = rawLine?.Trim();

                processedCount++;
                yield return normalisedRecord;
            }

            _logger.LogInformation("Successfully processed {Count} record(s) from {filePath}",
                processedCount, filePath);
        }

        private NormalisedRecord BuildNormalisedRecord(MD_Schema record)
        {
            return new NormalisedRecord
            {
                SourceKey = record.McDealerId,
                Name = record.SellerName,
                Postcode = record.Postcode?.Trim(),
                Domain = ExtractDomain(record.Website),

                Addresses = BuildAddressList(record),
                Contacts = BuildContactsList(record.Email, record.Phone)
            };
        }

        private List<Address> BuildAddressList(MD_Schema record)
        {
            if (string.IsNullOrWhiteSpace(record.Street) &&
                string.IsNullOrWhiteSpace(record.City) &&
                string.IsNullOrWhiteSpace(record.Postcode))
            {
                return new List<Address>();
            }

            return new List<Address>
            {
                new Address
                {
                    AddressLine1 = record.Street,
                    City = record.City,
                    County = record.County,
                    Postcode = record.Postcode?.Trim()
                }
            };
        }

        private List<Contact> BuildContactsList(string? email, string? phone)
        {
            var contacts = new List<Contact>();

            if (!string.IsNullOrWhiteSpace(email))
            {
                contacts.Add(new Contact { Value = email.Trim(), ContactType = ContactConstants.EMAIL });
            }

            if (!string.IsNullOrWhiteSpace(phone))
            {
                contacts.Add(new Contact { Value = phone.Trim(), ContactType = ContactConstants.TELEPHONE });
            }

            return contacts;
        }

        private string? ExtractDomain(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            var cleanUrl = url.Trim();

            if (!cleanUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !cleanUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                cleanUrl = "https://" + cleanUrl;
            }

            if (Uri.TryCreate(cleanUrl, UriKind.Absolute, out var uri))
            {
                return uri.Host.StartsWith("www.", StringComparison.OrdinalIgnoreCase)
                    ? uri.Host[4..]
                    : uri.Host;
            }

            return url;
        }
    }
}