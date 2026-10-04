using CsvHelper.Configuration.Attributes;

namespace DealerDatabase.Models.Schemas.CrawledDealers
{
    public class CD_Schema
    {
        [Name("crawl_id")]
        public string? CrawlId { get; set; }

        [Name("crawled_at")]
        public DateTimeOffset? CrawledAt { get; set; }

        [Name("source_url")]
        public string? SourceUrl { get; set; }

        [Name("final_url")]
        public string? FinalUrl { get; set; }

        [Name("http_status")]
        public int? HttpStatus { get; set; }

        [Name("page_type")]
        public string? PageType { get; set; }

        [Name("page_title")]
        public string? PageTitle { get; set; }

        [Name("h1")]
        public string? H1 { get; set; }

        [Name("meta_description")]
        public string? MetaDescription { get; set; }

        [Name("business_name_detected")]
        public string? BusinessNameDetected { get; set; }

        [Name("address_detected")]
        public string? AddressDetected { get; set; }

        [Name("postcode_detected")]
        public string? PostcodeDetected { get; set; }

        [Name("phones_detected")]
        public string? PhonesDetected { get; set; }

        [Name("emails_detected")]
        public string? EmailsDetected { get; set; }

        [Name("footer_text")]
        public string? FooterText { get; set; }

        [Name("company_number_detected")]
        public string? CompanyNumberDetected { get; set; }

        [Name("vat_number_detected")]
        public string? VatNumberDetected { get; set; }

        [Name("fca_frn_detected")]
        public string? FcaFrnDetected { get; set; }

        [Name("finance_calculator")]
        public string? FinanceCalculator { get; set; }

        [Name("finance_lenders_detected")]
        public string? FinanceLendersDetected { get; set; }

        [Name("representative_apr")]
        public string? RepresentativeApr { get; set; }

        [Name("website_platform")]
        public string? WebsitePlatform { get; set; }

        [Name("cms_version")]
        public string? CmsVersion { get; set; }

        [Name("google_rating")]
        public double? GoogleRating { get; set; }

        [Name("google_review_count")]
        public int? GoogleReviewCount { get; set; }

        [Name("trustpilot_score")]
        public double? TrustpilotScore { get; set; }

        [Name("facebook_url")]
        public string? FacebookUrl { get; set; }

        [Name("instagram_url")]
        public string? InstagramUrl { get; set; }

        [Name("opening_hours_raw")]
        public string? OpeningHoursRaw { get; set; }

        [Name("stock_count_detected")]
        public int? StockCountDetected { get; set; }

        [Name("part_exchange")]
        public string? PartExchange { get; set; }

        [Name("warranty_offered")]
        public string? WarrantyOffered { get; set; }

        [Name("cookie_banner")]
        public string? CookieBanner { get; set; }

        [Name("word_count")]
        public int? WordCount { get; set; }

        [Name("response_ms")]
        public int? ResponseMs { get; set; }

        [Name("content_hash")]
        public string? ContentHash { get; set; }

        [Name("lang")]
        public string? Lang { get; set; }
    }
}
