using DealerDatabase.Data.Entities;
using DealerDatabase.Models;
using DealerDatabase.Models.Constants;
using DealerDatabase.Models.Schemas.SAFMembers;
using DealerDatabase.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace DealerDatabase.Services.Readers
{
    public class SafMembersReader : IFileDataReader
    {
        private readonly ILogger<SafMembersReader> _logger;

        private static readonly XmlSerializer MemberSerializer = new(typeof(SafRegisterMember), new XmlRootAttribute("Member"));

        public SafMembersReader(ILogger<SafMembersReader> logger)
        {
            _logger = logger;
        }

        public IEnumerable<NormalisedRecord> ReadFile(string filePath)
        {
            _logger.LogDebug("Reading SAF Members XML file {filePath}", filePath);

            if (!File.Exists(filePath))
            {
                _logger.LogError("File not found: {filePath}", filePath);
                yield break;
            }

            XDocument doc;
            try
            {
                doc = XDocument.Load(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse XML file: {filePath}", filePath);
                yield break;
            }

            var memberElements = doc.Descendants()
                .Where(e => e.Name.LocalName.Equals("Member", StringComparison.OrdinalIgnoreCase) ||
                            e.Name.LocalName.Equals("SafRegisterMember", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!memberElements.Any())
            {
                _logger.LogWarning("No member nodes found in XML file: {filePath}", filePath);
                yield break;
            }

            int foundCount = memberElements.Count;
            _logger.LogInformation("Found {FoundCount} member record(s) in {filePath}",
                foundCount, filePath);

            foreach (var element in memberElements)
            {
                string rawXmlPayload = element.ToString(SaveOptions.DisableFormatting);

                SafRegisterMember? record;
                try
                {
                    using var reader = element.CreateReader();
                    record = (SafRegisterMember?)MemberSerializer.Deserialize(reader);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize SAF member XML node in {filePath}. Skipping.", filePath);
                    continue;
                }

                if (record == null) continue;

                var normalisedRecord = BuildNormalisedRecord(record);
                normalisedRecord.RawPayload = rawXmlPayload;

                yield return normalisedRecord;
            }
        }

        private NormalisedRecord BuildNormalisedRecord(SafRegisterMember record)
        {
            return new NormalisedRecord
            {
                SourceKey = record.Id,
                Name = !string.IsNullOrWhiteSpace(record.LegalName) ? record.LegalName : record.Name,
                Postcode = record.Postcode?.Trim(),
                Domain = ExtractDomain(record.Website),

                SafStatus = record.Status,
                SafExpiryDate = ParseExpiryDate(record.Expiry),

                Addresses = BuildAddressList(record),
                TradingNames = BuildTradingNamesList(record.TradingAs, record.Name),
                Contacts = BuildContactsList(record.Telephone)
            };
        }

        private DateTime? ParseExpiryDate(string? expiry)
        {
            if (string.IsNullOrWhiteSpace(expiry)) return null;

            // Handles ISO (2027-01-31), UK (22/12/2025), and text dates (16 Aug 2026, 5 March 2027)
            if (DateTime.TryParse(expiry, System.Globalization.CultureInfo.GetCultureInfo("en-GB"), System.Globalization.DateTimeStyles.None, out var parsedDate))
            {
                return parsedDate;
            }

            return null;
        }

        private List<Address> BuildAddressList(SafRegisterMember member)
        {
            if (string.IsNullOrWhiteSpace(member.Town) && string.IsNullOrWhiteSpace(member.Postcode))
            {
                return new List<Address>();
            }

            return new List<Address>
            {
                new Address
                {
                    City = member.Town,
                    Postcode = member.Postcode?.Trim()
                }
            };
        }

        private List<TradingName> BuildTradingNamesList(string? tradingAs, string? primaryName)
        {
            var tradingNames = new List<TradingName>();

            if (!string.IsNullOrWhiteSpace(tradingAs))
            {
                tradingNames.Add(new TradingName { Name = tradingAs.Trim() });
            }

            if (!string.IsNullOrWhiteSpace(primaryName) &&
                !tradingNames.Any(t => string.Equals(t.Name, primaryName, StringComparison.OrdinalIgnoreCase)))
            {
                tradingNames.Add(new TradingName { Name = primaryName.Trim() });
            }

            return tradingNames;
        }

        private List<Contact> BuildContactsList(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return new List<Contact>();
            }

            return new List<Contact>
            {
                new Contact { Value = phone.Trim(), ContactType = ContactConstants.TELEPHONE }
            };
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