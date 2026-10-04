using DealerDatabase.Data.Entities;
using DealerDatabase.Models;
using DealerDatabase.Models.Schemas.FcaRegister;
using DealerDatabase.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DealerDatabase.Services.Readers
{
    public class FcaRegisterReader : IFileDataReader
    {
        private readonly ILogger<FcaRegisterReader> _logger;

        public FcaRegisterReader(ILogger<FcaRegisterReader> logger)
        {
            _logger = logger;
        }

        public IEnumerable<NormalisedRecord> ReadFile(string filePath)
        {
            _logger.LogDebug("Reading FcaRegister file {filePath}", filePath);

            if (!File.Exists(filePath))
            {
                _logger.LogError("File not found: {filePath}", filePath);
                yield break;
            }

            string jsonContent = File.ReadAllText(filePath);

            FR_Schema? schema;
            try
            {
                schema = JsonSerializer.Deserialize<FR_Schema>(jsonContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize FCA Register file: {filePath}", filePath);
                yield break;
            }

            if (schema?.Data == null || schema.Data.Count == 0)
            {
                _logger.LogWarning("No data found in FCA Register file: {filePath}", filePath);
                yield break;
            }

            int foundCount = schema.Data.Count;
            string totalCountStr = schema.ResultInfo != null ? schema.ResultInfo.TotalCount.ToString() : "Not Known";

            _logger.LogInformation("Found {FoundCount} out of {TotalCount} record(s) from {filePath}",
                foundCount,
                totalCountStr, 
                filePath);

            foreach (var record in schema.Data)
            {
                var normalisedRecord = BuildNormalisedRecord(record);

                normalisedRecord.RawPayload = JsonSerializer.Serialize(record);

                yield return normalisedRecord;
            }
        }

        private NormalisedRecord BuildNormalisedRecord(CompanyData record)
        {
            var frnString = record.Frn > 0 ? record.Frn.ToString() : null;

            return new NormalisedRecord
            {
                SourceKey = frnString,
                Name = record.OrganisationName,
                CompanyNumber = record.CompaniesHouseNumber,
                FcaNumber = frnString,
                FcaStatus = record.Status,
                Postcode = record.Address?.Postcode?.Trim(),

                Addresses = BuildAddressList(record.Address),
                TradingNames = BuildTradingNamesList(record.TradingNames)
            };
        }

        private List<DealerDatabase.Data.Entities.Address> BuildAddressList(DealerDatabase.Models.Schemas.FcaRegister.Address? address)
        {
            if (address == null)
            {
                return new List<DealerDatabase.Data.Entities.Address>();
            }

            if (string.IsNullOrWhiteSpace(address.AddressLine1) &&
                string.IsNullOrWhiteSpace(address.Postcode))
            {
                return new List<DealerDatabase.Data.Entities.Address>();
            }

            return new List<DealerDatabase.Data.Entities.Address>
            {
                new DealerDatabase.Data.Entities.Address
                {
                    AddressLine1 = address.AddressLine1,
                    AddressLine2 = address.AddressLine2,
                    City = address.Town,
                    Postcode = address.Postcode?.Trim()
                }
            };
        }

        private List<TradingName> BuildTradingNamesList(List<string>? tradingNames)
        {
            if (tradingNames == null || !tradingNames.Any())
            {
                return new List<TradingName>();
            }

            return tradingNames
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => new TradingName { Name = name })
                .ToList();
        }
    }
}