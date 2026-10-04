using DealerDatabase.Models;
using DealerDatabase.Models.Schemas.VATLookups;
using DealerDatabase.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DealerDatabase.Services.Readers
{
    public class VatLookupReader : IFileDataReader
    {
        private readonly ILogger<VatLookupReader> _logger;

        public VatLookupReader(ILogger<VatLookupReader> logger)
        {
            _logger = logger;
        }

        public IEnumerable<NormalisedRecord> ReadFile(string filePath)
        {
            _logger.LogDebug("Reading VAT Lookup JSON file {filePath}", filePath);

            if (!File.Exists(filePath))
            {
                _logger.LogError("File not found: {filePath}", filePath);
                yield break;
            }

            string jsonContent = File.ReadAllText(filePath);
            using var doc = JsonDocument.Parse(jsonContent);
            var root = doc.RootElement;

            string rawJsonPayload = root.GetRawText();

            VL_Schema? record;
            try
            {
                record = root.Deserialize<VL_Schema>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize VAT Lookup payload in {filePath}", filePath);
                yield break;
            }

            if (record?.Target == null)
            {
                _logger.LogError("Unable to read valid target data for VAT file: {filePath}", filePath);
                yield break;
            }

            _logger.LogInformation("Successfully processed 1 VAT Lookup record from {filePath}", filePath);

            var normalisedRecord = BuildNormalisedRecord(record);
            normalisedRecord.RawPayload = rawJsonPayload;

            yield return normalisedRecord;
        }

        private NormalisedRecord BuildNormalisedRecord(VL_Schema record)
        {
            return new NormalisedRecord
            {
                SourceKey = record.Target.VatNumber,
                Name = record.Target.Name,
                VatNumber = record.Target.VatNumber,
                Postcode = record.Target.Address?.Postcode?.Trim(),

                Addresses = BuildAddressList(record.Target.Address)
            };
        }

        private List<DealerDatabase.Data.Entities.Address> BuildAddressList(DealerDatabase.Models.Schemas.VATLookups.Address? address)
        {
            if (address == null)
            {
                return new List<DealerDatabase.Data.Entities.Address>();
            }

            if (string.IsNullOrWhiteSpace(address.Line1) &&
                string.IsNullOrWhiteSpace(address.Postcode))
            {
                return new List<DealerDatabase.Data.Entities.Address>();
            }

            return new List<DealerDatabase.Data.Entities.Address>
            {
                new DealerDatabase.Data.Entities.Address
                {
                    AddressLine1 = address.Line1,
                    AddressLine2 = address.Line2,
                    AddressLine3 = address.Line3,
                    Postcode = address.Postcode?.Trim(),
                    Country = address.CountryCode
                }
            };
        }
    }
}