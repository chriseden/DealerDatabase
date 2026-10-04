using DealerDatabase.DataAccess.Interfaces;
using DealerDatabase.Models;
using DealerDatabase.Services.Interfaces;
using Microsoft.Extensions.Logging;


namespace DealerDatabase.Services.Services
{
    public class DataOrchestrator
    {
        private readonly IDynamicReaderFactory _readerFactory;
        private readonly IIngestionService _ingestionService;
        private readonly IDealerRepository _dealerRepository;
        private readonly ILogger<DataOrchestrator> _logger;

        public DataOrchestrator(
            IDynamicReaderFactory readerFactory,
            IIngestionService ingestionService,
            IDealerRepository dealerRepository,
            ILogger<DataOrchestrator> logger)
        {
            _readerFactory = readerFactory;
            _ingestionService = ingestionService;
            _dealerRepository = dealerRepository;
            _logger = logger;
        }

        public async Task ExecutePipelineAsync(string inputPath, IProgress<ProgressReport>? progress = null)
        {
            if (string.IsNullOrWhiteSpace(inputPath))
            {
                throw new ArgumentException("Input path cannot be null or empty.", nameof(inputPath));
            }

            _logger.LogInformation("Starting data ingestion process for path: {Path}", inputPath);

            int totalRecordsProcessed = 0;

            if (File.Exists(inputPath))
            {
                progress?.Report(new ProgressReport
                {
                    FileIndex = 1,
                    TotalFiles = 1,
                    FilePath = inputPath,
                    TotalRecordsProcessed = 0,
                    StatusMessage = "Processing single file..."
                });

                totalRecordsProcessed += await ProcessSingleFileAsync(inputPath);
            }
            else if (Directory.Exists(inputPath))
            {
                var options = new EnumerationOptions
                {
                    RecurseSubdirectories = true,
                    IgnoreInaccessible = true,
                    AttributesToSkip = FileAttributes.Hidden | FileAttributes.System
                };

                var files = Directory.EnumerateFiles(inputPath, "*.*", options)
                    .OrderBy(GetFileProcessingPriority)
                    .ThenBy(filePath => filePath)
                    .ToList();

                int totalFiles = files.Count;
                int currentFileIndex = 0;

                foreach (string filePath in files)
                {
                    currentFileIndex++;

                    progress?.Report(new ProgressReport
                    {
                        FileIndex = currentFileIndex,
                        TotalFiles = totalFiles,
                        FilePath = filePath,
                        TotalRecordsProcessed = totalRecordsProcessed,
                        StatusMessage = $"Processing {Path.GetFileName(filePath)}"
                    });

                    int recordsInFile = await ProcessSingleFileAsync(filePath);
                    totalRecordsProcessed += recordsInFile;

                    progress?.Report(new ProgressReport
                    {
                        FileIndex = currentFileIndex,
                        TotalFiles = totalFiles,
                        FilePath = filePath,
                        TotalRecordsProcessed = totalRecordsProcessed,
                        StatusMessage = $"Finished with {Path.GetFileName(filePath)}"
                    });
                }
            }
            else
            {
                throw new FileNotFoundException($"Path does not exist: {inputPath}");
            }

            // Database Sync Progress
            progress?.Report(new ProgressReport
            {
                FileIndex = 0,
                TotalFiles = 0,
                FilePath = string.Empty,
                TotalRecordsProcessed = totalRecordsProcessed,
                StatusMessage = "Saving records to database..."
            });

            _dealerRepository.FillActiveBuckets(new List<DealerBucket>(_ingestionService.ActiveBuckets));
            await _dealerRepository.SaveBucketsToDatabaseAsync();

            progress?.Report(new ProgressReport
            {
                FileIndex = 0,
                TotalFiles = 0,
                FilePath = string.Empty,
                TotalRecordsProcessed = totalRecordsProcessed,
                StatusMessage = "Process complete."
            });
        }

        private async Task<int> ProcessFolderAsync(string inputPath)
        {
            int totalRecordsProcessed = 0;

            var options = new EnumerationOptions
            {
                RecurseSubdirectories = true,
                IgnoreInaccessible = true,
                AttributesToSkip = FileAttributes.Hidden | FileAttributes.System
            };

            // Retrieve and order files so primary authority sources process first
            var files = Directory.EnumerateFiles(inputPath, "*.*", options)
                .OrderBy(GetFileProcessingPriority)
                .ThenBy(filePath => filePath)
                .ToList();

            int totalFiles = files.Count;

            _logger.LogInformation("Discovered {TotalFiles} file(s) across directory tree: {Path}", totalFiles, inputPath);

            int currentFileIndex = 0;

            foreach (string filePath in files)
            {
                currentFileIndex++;

                _logger.LogInformation("[{Current}/{Total}] Processing file: {FilePath}",
                    currentFileIndex,
                    totalFiles,
                    filePath);

                int recordsInFile = await ProcessSingleFileAsync(filePath);
                totalRecordsProcessed += recordsInFile;

                _logger.LogDebug("[{Current}/{Total}] Finished {FilePath} - Extracted {Count} record(s). Total so far: {TotalRecords}",
                    currentFileIndex,
                    totalFiles,
                    filePath,
                    recordsInFile,
                    totalRecordsProcessed);
            }

            _logger.LogInformation("Batch complete. Processed {TotalFiles} file(s) and {TotalRecords} total record(s).",
                totalFiles,
                totalRecordsProcessed);

            return totalRecordsProcessed;
        }

        private async Task<int> ProcessSingleFileAsync(string filePath)
        {
            int recordsInFile = 0;

            try
            {
                var reader = _readerFactory.GetReader(filePath).FirstOrDefault();

                if (reader == null)
                {
                    _logger.LogWarning("No suitable reader found for file: {FileName}. Skipping.", Path.GetFileName(filePath));
                    return 0;
                }

                _logger.LogInformation("Processing {FileName} using {ReaderType}...",
                    Path.GetFileName(filePath), reader.GetType().Name);

                foreach (NormalisedRecord record in reader.ReadFile(filePath))
                {
                    if (record != null)
                    {
                        await _ingestionService.ProcessRecordAsync(record);
                        recordsInFile++;
                    }
                }

                _logger.LogInformation("Successfully ingested {Count} record(s) from {FileName}.",
                    recordsInFile, Path.GetFileName(filePath));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing file: {FileName}. Continuing with next file.", Path.GetFileName(filePath));
            }

            return recordsInFile;
        }

        private int GetFileProcessingPriority(string filePath)
        {
            string fileName = Path.GetFileName(filePath);

            // Priority 1: Primary Corporate Identity Registry
            if (fileName.Contains("CompanyHouse", StringComparison.OrdinalIgnoreCase) ||
                filePath.Contains("CompaniesHouse", StringComparison.OrdinalIgnoreCase))
                return 1;

            // Priority 2: Financial Regulatory Register
            if (fileName.Contains("FcaRegister", StringComparison.OrdinalIgnoreCase) ||
                filePath.Contains("FCA", StringComparison.OrdinalIgnoreCase))
                return 2;

            // Priority 3: Tax / VAT Register 
            if (fileName.Contains("VatLookup", StringComparison.OrdinalIgnoreCase) ||
                filePath.Contains("VAT", StringComparison.OrdinalIgnoreCase))
                return 3;

            // Priority 4: Data Protection Licensing
            if (fileName.Contains("IcoRegister", StringComparison.OrdinalIgnoreCase) ||
                filePath.Contains("ICO", StringComparison.OrdinalIgnoreCase))
                return 4;

            // Priority 10: Fallback for crawling data, web scrapes, or secondary sources
            return 10;
        }
    }
}