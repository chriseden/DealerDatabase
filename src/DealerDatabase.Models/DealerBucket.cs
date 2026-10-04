

namespace DealerDatabase.Models
{
    public class DealerBucket
    {
        public DealerBucket(NormalisedRecord initialRecord)
        {
            AddOrUpdateRecord(initialRecord);
        }

        public List<NormalisedRecord> MemberRecords { get; } = new();
        public NormalisedRecord RepresentativeRecord { get; private set; }
        public int? ExistingDealerId { get; set; }

        public void AddOrUpdateRecord(NormalisedRecord record)
        {
            if (record == null) return;

            MemberRecords.RemoveAll(r => r.SourceKey == record.SourceKey);
            MemberRecords.Add(record);

            RebuildRepresentativeRecord();
        }

        private void RebuildRepresentativeRecord()
        {
            RepresentativeRecord = new NormalisedRecord
            {
                SourceKey = MemberRecords.FirstOrDefault()?.SourceKey,
                Name = ResolveField(r => r.Name, "Name"),
                CompanyNumber = ResolveField(r => r.CompanyNumber, "CompanyNumber"),
                FcaNumber = ResolveField(r => r.FcaNumber, "FcaNumber"),
                VatNumber = ResolveField(r => r.VatNumber, "VatNumber"),
                Postcode = ResolveField(r => r.Postcode, "Postcode"),
                Domain = ResolveField(r => r.Domain, "Domain"),

                IncorporationDate = ResolveDateField(r => r.IncorporationDate, "IncorporationDate"),
                CompanyStatus = ResolveField(r => r.CompanyStatus, "CompanyStatus"),
                FcaStatus = ResolveField(r => r.FcaStatus, "FcaStatus"),

                IcoRegistrationNumber = ResolveField(r => r.IcoRegistrationNumber, "IcoRegistrationNumber"),
                IcoExpiryDate = ResolveDateField(r => r.IcoExpiryDate, "IcoExpiryDate"),

                SafStatus = ResolveField(r => r.SafStatus, "SafStatus"),
                SafExpiryDate = ResolveDateField(r => r.SafExpiryDate, "SafExpiryDate"),

                Addresses = MemberRecords.SelectMany(r => r.Addresses ?? Enumerable.Empty<Data.Entities.Address>()).Distinct().ToList(),
                TradingNames = MemberRecords.SelectMany(r => r.TradingNames ?? Enumerable.Empty<Data.Entities.TradingName>()).Distinct().ToList(),
                Contacts = MemberRecords.SelectMany(r => r.Contacts ?? Enumerable.Empty<Data.Entities.Contact>()).Distinct().ToList(),
                Directors = MemberRecords.SelectMany(r => r.Directors ?? Enumerable.Empty<Data.Entities.Director>()).Distinct().ToList()
            };
        }

        private DateTime? ResolveDateField(Func<NormalisedRecord, DateTime?> dateSelector, string fieldName)
        {
            return MemberRecords
                .Where(r => dateSelector(r).HasValue)
                .OrderBy(r => GetSourcePriority(r.SourceKey ?? string.Empty, fieldName))
                .Select(dateSelector)
                .FirstOrDefault();
        }


        private string? ResolveField(Func<NormalisedRecord, string?> fieldSelector, string fieldName)
        {
            return MemberRecords
                .Where(r => !string.IsNullOrWhiteSpace(fieldSelector(r)))
                .OrderBy(r => GetSourcePriority(r.SourceKey ?? string.Empty, fieldName))
                .ThenByDescending(r => fieldSelector(r)!.Length) 
                .Select(fieldSelector)
                .FirstOrDefault();
        }

        private int GetSourcePriority(string sourceKey, string fieldName)
        {
            return (fieldName, sourceKey) switch
            {
                // Companies House is authority for Company Number & Status
                ("CompanyNumber", var s) when s.Contains("CompanyHouse", StringComparison.OrdinalIgnoreCase) => 1,
                ("CompanyStatus", var s) when s.Contains("CompanyHouse", StringComparison.OrdinalIgnoreCase) => 1,

                // FCA Register is authority for FCA Number & FRN Status
                ("FcaNumber", var s) when s.Contains("FcaRegister", StringComparison.OrdinalIgnoreCase) => 1,
                ("FcaStatus", var s) when s.Contains("FcaRegister", StringComparison.OrdinalIgnoreCase) => 1,

                // VAT Lookup is authority for VAT Number
                ("VatNumber", var s) when s.Contains("VatLookup", StringComparison.OrdinalIgnoreCase) => 1,

                // ICO Register is authority for ICO Number
                ("IcoRegistrationNumber", var s) when s.Contains("IcoRegister", StringComparison.OrdinalIgnoreCase) => 1,

                // Web Crawlers / Marketcheck are secondary sources
                (_, var s) when s.Contains("CrawledDealers", StringComparison.OrdinalIgnoreCase) => 8,
                (_, var s) when s.Contains("Marketcheck", StringComparison.OrdinalIgnoreCase) => 9,

                _ => 10 
            };
        }

        public string BuildLineageTag()
        {
            return string.Join("|", MemberRecords.Select(r => r.SourceKey).Where(k => !string.IsNullOrEmpty(k)));
        }
        public string ResolveBestName()
        {
            // Try the Name   
            if (!string.IsNullOrWhiteSpace(RepresentativeRecord.Name))
            {
                return RepresentativeRecord.Name.Trim();
            }

            // Ok, next try the first Trading Name
            var tradingName = RepresentativeRecord.TradingNames
                .FirstOrDefault(t => !string.IsNullOrWhiteSpace(t.Name))?.Name;

            if (!string.IsNullOrWhiteSpace(tradingName))
            {
                return tradingName.Trim();
            }

            // Ok, Lets try the Domain name
            if (!string.IsNullOrWhiteSpace(RepresentativeRecord.Domain))
            {
                return FormatDomainAsName(RepresentativeRecord.Domain);
            }

            // Oh dear, it's Unknown
            return $"Unknown Dealer ({BuildLineageTag()})";
        }

        private string FormatDomainAsName(string domain)
        {

            string clean = domain.Replace("https://", "", StringComparison.OrdinalIgnoreCase)
                                .Replace("http://", "", StringComparison.OrdinalIgnoreCase)
                                .Replace("www.", "", StringComparison.OrdinalIgnoreCase);


            int firstDot = clean.IndexOf('.');
            if (firstDot > 0)
            {
                clean = clean[..firstDot];
            }

            return char.ToUpper(clean[0]) + clean[1..];
        }
    }
}
