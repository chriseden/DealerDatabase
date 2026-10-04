using CsvHelper.Configuration.Attributes;


namespace DealerDatabase.Models.Schemas.IcoRegiser
{
    public class IR_Schema
    {
        [Name("Registration_number")]
        public string? RegistrationNumber { get; set; }

        [Name("Organisation_name")]
        public string? OrganisationName { get; set; }

        [Name("Company_registration_number")]
        public string? CompanyRegistrationNumber { get; set; }

        [Name("Organisation_address_line_1")]
        public string? OrganisationAddressLine1 { get; set; }

        [Name("Organisation_address_line_2")]
        public string? OrganisationAddressLine2 { get; set; }

        [Name("Organisation_address_line_3")]
        public string? OrganisationAddressLine3 { get; set; }

        [Name("Organisation_postcode")]
        public string? OrganisationPostcode { get; set; }

        [Name("Public_authority")]
        public string? PublicAuthority { get; set; }

        [Name("Start_date_of_registration")]
        [Format("dd/MM/yyyy")]
        public DateTime? StartDateOfRegistration { get; set; }

        [Name("End_date_of_registration")]
        [Format("dd/MM/yyyy")]
        public DateTime? EndDateOfRegistration { get; set; }

        [Name("Payment_tier")]
        public string? PaymentTier { get; set; }

        [Name("Trading_names")]
        public string? TradingNames { get; set; }
    }
}