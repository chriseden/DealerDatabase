using CsvHelper;
using CsvHelper.Configuration;
using DealerDatabase.Data.Entities;
using DealerDatabase.Models;
using DealerDatabase.Models.Schemas.IcoRegiser;
using DealerDatabase.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace DealerDatabase.Services.Readers
{
    public class IcoRegisterReader : IFileDataReader
    {
        private readonly ILogger<IcoRegisterReader> _logger;

        public IcoRegisterReader(ILogger<IcoRegisterReader> logger)
        {
            _logger = logger;
        }

        public IEnumerable<NormalisedRecord> ReadFile(string filePath)
        {
            _logger.LogDebug("Reading ICO Register CSV file {filePath}", filePath);

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
                // Capture exact raw CSV line
                string rawLine = csv.Parser.RawRecord;

                IR_Schema? record;
                try
                {
                    record = csv.GetRecord<IR_Schema>();
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

        private NormalisedRecord BuildNormalisedRecord(IR_Schema record)
        {
            return new NormalisedRecord
            {
                SourceKey = record.RegistrationNumber,
                Name = record.OrganisationName,
                CompanyNumber = record.CompanyRegistrationNumber,
                IcoRegistrationNumber = record.RegistrationNumber,
                IcoExpiryDate = record.EndDateOfRegistration,
                Postcode = record.OrganisationPostcode,

                Addresses = BuildAddressList(record),
                TradingNames = BuildTradingNamesList(record.TradingNames)
            };
        }

        private List<Address> BuildAddressList(IR_Schema record)
        {
            if (string.IsNullOrWhiteSpace(record.OrganisationAddressLine1) &&
                string.IsNullOrWhiteSpace(record.OrganisationPostcode))
            {
                return new List<Address>();
            }

            return new List<Address>
            {
                new Address
                {
                    AddressLine1 = record.OrganisationAddressLine1,
                    AddressLine2 = record.OrganisationAddressLine2,
                    AddressLine3 = record.OrganisationAddressLine3,
                    Postcode = record.OrganisationPostcode
                }
            };
        }

        private List<TradingName> BuildTradingNamesList(string? rawTradingNames)
        {
            if (string.IsNullOrWhiteSpace(rawTradingNames))
            {
                return new List<TradingName>();
            }

            return rawTradingNames
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(name => new TradingName { Name = name })
                .ToList();
        }
    }
}