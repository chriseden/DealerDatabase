using DealerDatabase.Models;
using DealerDatabase.Services.Interfaces;
using System;
using System.Text.RegularExpressions;

namespace DealerDatabase.Services.Services
{
    public class MatchingService : IMatchingService
    {
        public double CalculateMatchScore(NormalisedRecord recordA, NormalisedRecord recordB)
        {
            // 1. Hard Deterministic Matches (Highest Confidence & Case-Insensitive)
            if (CompareIdentifiers(recordA.CompanyNumber, recordB.CompanyNumber))
            {
                return 1.0;
            }

            if (CompareIdentifiers(recordA.FcaNumber, recordB.FcaNumber))
            {
                return 1.0;
            }

            if (CompareVatNumbers(recordA.VatNumber, recordB.VatNumber))
            {
                return 0.98;
            }

            // 2. Probabilistic Matching (Name + Location / Domain)
            double nameScore = GetNameSimilarity(recordA.Name, recordB.Name);
            bool exactPostcodeMatch = ComparePostcodes(recordA.Postcode, recordB.Postcode);
            bool outwardPostcodeMatch = ComparePostcodeOutward(recordA.Postcode, recordB.Postcode);
            bool domainMatch = CompareDomains(recordA.Domain, recordB.Domain);

            // Exact postcode boost
            if (exactPostcodeMatch && nameScore >= 0.80)
            {
                return Math.Min(1.0, nameScore + 0.15);
            }

            // Outward postcode sector boost 
            if (outwardPostcodeMatch && nameScore >= 0.85)
            {
                return Math.Min(1.0, nameScore + 0.10);
            }

            // Domain boost
            if (domainMatch && nameScore >= 0.75)
            {
                return Math.Min(1.0, nameScore + 0.20);
            }

            // High-confidence name match safety net (preserves strong name matches even if postcodes differ)
            if (nameScore >= 0.92)
            {
                return nameScore;
            }

            return nameScore * 0.7; // Soft fallback
        }

        private bool CompareIdentifiers(string? id1, string? id2)
        {
            if (string.IsNullOrWhiteSpace(id1) || string.IsNullOrWhiteSpace(id2)) return false;

            string clean1 = Regex.Replace(id1.Trim(), @"\s+", "").ToUpperInvariant();
            string clean2 = Regex.Replace(id2.Trim(), @"\s+", "").ToUpperInvariant();

            return clean1 == clean2;
        }

        private bool CompareVatNumbers(string? vat1, string? vat2)
        {
            if (string.IsNullOrWhiteSpace(vat1) || string.IsNullOrWhiteSpace(vat2))
                return false;

            string clean1 = CleanVat(vat1);
            string clean2 = CleanVat(vat2);

            return !string.IsNullOrEmpty(clean1) && clean1 == clean2;
        }

        private string CleanVat(string vat)
        {
            string cleaned = vat.Trim().ToUpperInvariant();

            // Strip "GB" prefix if present
            if (cleaned.StartsWith("GB"))
            {
                cleaned = cleaned[2..];
            }

            // Remove spaces, punctuation, or non-alphanumeric chars
            return Regex.Replace(cleaned, @"[^\w]", "");
        }

        private bool ComparePostcodes(string? pc1, string? pc2)
        {
            if (string.IsNullOrWhiteSpace(pc1) || string.IsNullOrWhiteSpace(pc2)) return false;
            return CleanPostcode(pc1) == CleanPostcode(pc2);
        }

        private bool ComparePostcodeOutward(string? pc1, string? pc2)
        {
            if (string.IsNullOrWhiteSpace(pc1) || string.IsNullOrWhiteSpace(pc2)) return false;

            string c1 = CleanPostcode(pc1);
            string c2 = CleanPostcode(pc2);

            if (c1.Length < 4 || c2.Length < 4) return false;

            // Extract outward area (everything except the last 3 inward characters)
            string out1 = c1[..^3];
            string out2 = c2[..^3];

            return out1 == out2;
        }

        private bool CompareDomains(string? d1, string? d2)
        {
            if (string.IsNullOrWhiteSpace(d1) || string.IsNullOrWhiteSpace(d2)) return false;
            return string.Equals(d1.Trim(), d2.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private double GetNameSimilarity(string? name1, string? name2)
        {
            if (string.IsNullOrWhiteSpace(name1) || string.IsNullOrWhiteSpace(name2)) return 0.0;

            string n1 = CleanCompanyName(name1);
            string n2 = CleanCompanyName(name2);

            if (n1 == n2) return 1.0;

            return JaroWinklerDistance(n1, n2);
        }

        public string CleanCompanyName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;

            string cleaned = name.ToLowerInvariant();
            cleaned = Regex.Replace(cleaned, @"\b(limited|ltd|plc|uk|motors|motor|group|cars|auto|car)\b", "");
            cleaned = Regex.Replace(cleaned, @"[^\w\s]", "");
            cleaned = Regex.Replace(cleaned, @"\s+", " "); // Normalize multi-space gaps
            return cleaned.Trim();
        }

        public string CleanPostcode(string postcode)
        {
            if (string.IsNullOrWhiteSpace(postcode)) return string.Empty;
            return Regex.Replace(postcode.ToUpperInvariant(), @"\s+", "");
        }

        // Standard Jaro-Winkler implementation for fuzzy string comparisons
        private double JaroWinklerDistance(string s1, string s2)
        {
            int m = 0;
            if (s1.Length == 0 || s2.Length == 0) return 0.0;
            if (s1 == s2) return 1.0;

            int range = (Math.Max(s1.Length, s2.Length) / 2) - 1;
            if (range < 0) range = 0;

            bool[] s1Matches = new bool[s1.Length];
            bool[] s2Matches = new bool[s2.Length];

            for (int i = 0; i < s1.Length; i++)
            {
                int low = (i >= range) ? i - range : 0;
                int high = (i + range >= s2.Length) ? s2.Length - 1 : i + range;

                for (int j = low; j <= high; j++)
                {
                    if (s2Matches[j] || s1[i] != s2[j]) continue;
                    s1Matches[i] = true;
                    s2Matches[j] = true;
                    m++;
                    break;
                }
            }

            if (m == 0) return 0.0;

            double k = 0;
            int numTrans = 0;
            for (int i = 0; i < s1.Length; i++)
            {
                if (!s1Matches[i]) continue;
                while (!s2Matches[(int)k]) k++;
                if (s1[i] != s2[(int)k]) numTrans++;
                k++;
            }

            double jaro = (m / (double)s1.Length + m / (double)s2.Length + (m - (numTrans / 2.0)) / m) / 3.0;

            // Winkler prefix scaling
            int prefix = 0;
            for (int i = 0; i < Math.Min(4, Math.Min(s1.Length, s2.Length)); i++)
            {
                if (s1[i] == s2[i]) prefix++;
                else break;
            }

            return jaro + (prefix * 0.1 * (1.0 - jaro));
        }
    }
}