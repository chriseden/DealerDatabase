using DealerDatabase.Services.Factory;
using DealerDatabase.Services.Interfaces;
using DealerDatabase.Services.Readers;
using Microsoft.Extensions.DependencyInjection;


namespace DealerDatabase.Services.Configuration
{
    public static class DataReaderConfiguration
    {
        public static IServiceCollection DataReaderSetup(this IServiceCollection services)
        {

            services.AddKeyedTransient<IFileDataReader, CrawlerDealersReader>("CrawledDealers_CSV");
            services.AddKeyedTransient<IFileDataReader, IcoRegisterReader>("IcoRegister_CSV");
            services.AddKeyedTransient<IFileDataReader, MarketcheckReader>("Marketcheck_CSV");
            services.AddKeyedTransient<IFileDataReader, CompanyHouseReader>("CompanyHouse_JSON");
            services.AddKeyedTransient<IFileDataReader, FcaRegisterReader>("FcaRegister_JSON");
            services.AddKeyedTransient<IFileDataReader, VatLookupReader>("VatLookup_JSON");
            services.AddKeyedTransient<IFileDataReader, SafMembersReader>("SafMembers_XML");

            services.AddTransient<IDynamicReaderFactory, DynamicReaderFactory>();

            return services;
        }
    }
}

