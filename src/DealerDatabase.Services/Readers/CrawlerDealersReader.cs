using CsvHelper;
using CsvHelper.Configuration;
using DealerDatabase.Data.Entities;
using DealerDatabase.Models;
using DealerDatabase.Models.Constants;
using DealerDatabase.Models.Schemas.CrawledDealers;
using DealerDatabase.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace DealerDatabase.Services.Readers
{
    public class CrawlerDealersReader : IFileDataReader
    {
        private readonly ILogger<CrawlerDealersReader> _logger;

        public CrawlerDealersReader(ILogger<CrawlerDealersReader> logger)
        {
            _logger = logger;
        }

        public IEnumerable<NormalisedRecord> ReadFile(string filePath)
        {
            _logger.LogDebug("Reading CrawlerDealers file {filePath}", filePath);

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

            // Read header row
            if (!csv.Read() || !csv.ReadHeader())
            {
                _logger.LogWarning("CSV file is empty or missing headers: {filePath}", filePath);
                yield break;
            }

            int processedCount = 0;

            while (csv.Read())
            {
                // Capture the exact raw CSV line
                string rawLine = csv.Parser.RawRecord;

                CD_Schema record;
                try
                {
                    record = csv.GetRecord<CD_Schema>();
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

            _logger.LogInformation("Successfully processed {Count} record(s)) from {filePath}",
                processedCount, filePath);
        }

        private NormalisedRecord BuildNormalisedRecord(CD_Schema record)
        {
            return new NormalisedRecord
            {
                SourceKey = record.CrawlId,
                Name = record.BusinessNameDetected,
                CompanyNumber = record.CompanyNumberDetected,
                FcaNumber = record.FcaFrnDetected,
                VatNumber = record.VatNumberDetected,
                Postcode = record.PostcodeDetected,
                Domain = ExtractDomain(record.FinalUrl ?? record.SourceUrl),

                Addresses = BuildAddressList(record.AddressDetected, record.PostcodeDetected),
                Contacts = BuildContactsList(record.EmailsDetected, record.PhonesDetected)
            };
        }

        private List<Address> BuildAddressList(string? rawAddress, string? postcode)
        {
            if (string.IsNullOrWhiteSpace(rawAddress) && string.IsNullOrWhiteSpace(postcode))
            {
                return new List<Address>();
            }

            return new List<Address>
            {
                new Address
                {
                    RawAddress = rawAddress,
                    Postcode = postcode
                }
            };
        }

        private List<Contact> BuildContactsList(string? emailsRaw, string? phonesRaw)
        {
            var contacts = new List<Contact>();

            // Split emails (comma or semicolon delimited)
            if (!string.IsNullOrWhiteSpace(emailsRaw))
            {
                var emails = emailsRaw.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var email in emails)
                {
                    contacts.Add(new Contact { Value = email, ContactType = ContactConstants.EMAIL });
                }
            }

            // Split phone numbers (comma or semicolon delimited)
            if (!string.IsNullOrWhiteSpace(phonesRaw))
            {
                var phones = phonesRaw.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var phone in phones)
                {
                    contacts.Add(new Contact { Value = phone, ContactType = ContactConstants.TELEPHONE });
                }
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