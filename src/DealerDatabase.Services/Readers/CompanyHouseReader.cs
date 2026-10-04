using DealerDatabase.Data.Entities;
using DealerDatabase.Models;
using DealerDatabase.Models.Constants;
using DealerDatabase.Models.Schemas.CompanyHouse;
using DealerDatabase.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DealerDatabase.Services.Readers
{
    public class CompanyHouseReader : IFileDataReader
    {
        private readonly ILogger<CompanyHouseReader> _logger;

        public CompanyHouseReader(ILogger<CompanyHouseReader> logger)
        {
            _logger = logger;
        }

        public IEnumerable<NormalisedRecord> ReadFile(string filePath)
        {
            _logger.LogDebug("Reading Companies House file {filePath}", filePath);

            if (!File.Exists(filePath))
            {
                _logger.LogError("File not found: {filePath}", filePath);
                yield break;
            }

            string jsonContent = File.ReadAllText(filePath);
            using var doc = JsonDocument.Parse(jsonContent);
            var root = doc.RootElement;

            if (!root.TryGetProperty("items", out var itemsElement) || itemsElement.ValueKind != JsonValueKind.Array)
            {
                _logger.LogError("Error: Unable to locate 'items' array in JSON file: {filePath}", filePath);
                yield break;
            }

            int foundCount = itemsElement.GetArrayLength();
            int totalCount = root.TryGetProperty("items_count", out var totalElement) ? totalElement.GetInt32() : foundCount;
            _logger.LogInformation("Found {FoundCount} out of {TotalCount} record(s) from {filePath}",
                foundCount,
                totalCount,
                filePath);

            foreach (var itemNode in itemsElement.EnumerateArray())
            {
                // Capture exact raw JSON fragment for this specific item node
                string rawJsonPayload = itemNode.GetRawText();

                // Deserialize item node into strong DTO
                var record = itemNode.Deserialize<Item>();
                if (record == null) continue;

                var normalisedRecord = BuildNormalisedRecord(record);
                normalisedRecord.RawPayload = rawJsonPayload;

                yield return normalisedRecord;
            }
        }

        private NormalisedRecord BuildNormalisedRecord(Item record)
        {
            return new NormalisedRecord
            {
                SourceKey = record.CompanyNumber,
                Name = record.CompanyName,
                CompanyNumber = record.CompanyNumber,
                CompanyStatus = record.CompanyStatus,

                IncorporationDate = DateTime.TryParse(record.DateOfCreation, out var creationDate)
                    ? creationDate
                    : null,

                Postcode = record.RegisteredOfficeAddress?.PostalCode,

                Addresses = BuildAddressList(record.RegisteredOfficeAddress),
                TradingNames = BuildTradingNamesList(record.PreviousCompanyNames),
                Directors = BuildDirectorsList(record.Officers)
            };
        }

        private List<Address> BuildAddressList(RegisteredOfficeAddress? officeAddress)
        {
            if (officeAddress == null)
            {
                return new List<Address>();
            }

            return new List<Address>
            {
                new Address
                {
                    AddressLine1 = officeAddress.AddressLine1,
                    AddressLine2 = officeAddress.AddressLine2,
                    City = officeAddress.Locality,
                    County = officeAddress.Region,
                    Postcode = officeAddress.PostalCode,
                    Country = officeAddress.Country,
                    AddressType = AddressConstants.REGISTERED
                }
            };
        }

        private List<TradingName> BuildTradingNamesList(List<PreviousCompanyName> previousNames)
        {
            if (previousNames == null)
            {
                return new List<TradingName>();
            }

            return previousNames
                .Where(p => !string.IsNullOrWhiteSpace(p.Name))
                .Select(p => new TradingName { Name = p.Name })
                .ToList();
        }

        private List<Director> BuildDirectorsList(List<Officer>? officers)
        {
            if (officers == null)
            {
                return new List<Director>();
            }

            return officers
                .Where(o => !string.IsNullOrWhiteSpace(o.Name))
                .Select(o => new Director
                {
                    FullName = o.Name,
                    Role = o.OfficerRole,
                    AppointedOn = DateTime.TryParse(o.AppointedOn, out var appDate) ? appDate : null,
                    ResignedOn = DateTime.TryParse(o.ResignedOn, out var resDate) ? resDate : null
                })
                .ToList();
        }
    }
}