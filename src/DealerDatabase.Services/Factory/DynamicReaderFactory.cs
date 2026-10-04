using DealerDatabase.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace DealerDatabase.Services.Factory
{
    public class DynamicReaderFactory : IDynamicReaderFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public DynamicReaderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IEnumerable<IFileDataReader> GetReader(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path cannot be null or empty.", nameof(path));
            }

            if (File.Exists(path))
            {
                yield return GetReaderForFile(path);
            }
            else if (Directory.Exists(path))
            {
                foreach (string file in Directory.GetFiles(path))
                {
                    yield return GetReaderForFile(file);
                }
            }
            else
            {
                throw new FileNotFoundException($"The specified file or directory path does not exist: {path}");
            }
        }

        private IFileDataReader GetReaderForFile(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();

            if (extension == ".csv")
            {
                using var reader = new StreamReader(filePath);
                string header = reader.ReadLine() ?? "";

                if (header.Contains("crawl_id") && header.Contains("crawled_at"))
                    return _serviceProvider.GetRequiredKeyedService<IFileDataReader>("CrawledDealers_CSV");

                if (header.Contains("Registration_number") && header.Contains("Organisation_name"))
                    return _serviceProvider.GetRequiredKeyedService<IFileDataReader>("IcoRegister_CSV");

                if (header.Contains("mc_dealer_id") && header.Contains("seller_name"))
                    return _serviceProvider.GetRequiredKeyedService<IFileDataReader>("Marketcheck_CSV");
            }
            else if (extension == ".json")
            {
                string jsonContent = File.ReadAllText(filePath);
                using var doc = JsonDocument.Parse(jsonContent);
                var root = doc.RootElement;

                if (root.TryGetProperty("company_number", out _) || root.TryGetProperty("items", out _))
                    return _serviceProvider.GetRequiredKeyedService<IFileDataReader>("CompanyHouse_JSON");

                if (root.TryGetProperty("ResultInfo", out _) || root.TryGetProperty("Data", out _))
                    return _serviceProvider.GetRequiredKeyedService<IFileDataReader>("FcaRegister_JSON");

                if (root.TryGetProperty("target", out _) && root.TryGetProperty("processingDate", out _))
                    return _serviceProvider.GetRequiredKeyedService<IFileDataReader>("VatLookup_JSON");
            }
            else if (extension == ".xml")
            {
                using var reader = new StreamReader(filePath);
                string content = reader.ReadToEnd();

                if (content.Contains("<SafRegister") || content.Contains("<Member"))
                    return _serviceProvider.GetRequiredKeyedService<IFileDataReader>("SafMembers_XML");
            }

            throw new NotSupportedException($"No parser registered to handle file: {Path.GetFileName(filePath)}");
        }
    }
}
