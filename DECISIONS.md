# Data Ingestion & Reconciliation Decisions

## 1. Matching & Deduplication Approach

I use a **two-tiered matching strategy** inside an `IngestionService` to identify duplicate records and merge them into canonical `DealerBucket` clusters:

* **Tier 1: Exact Key Matching**
  * **Rule:** If an incoming record matches an existing record's explicit identifier (`CompanyNumber`, `FcaNumber`, `VatNumber`, or exact `SourceKey`), it immediately merges into that dealer bucket without scoring.

* **Tier 2: Weighted Fuzzy Matching (Probabilistic)**
  * **Rule:** For records lacking hard key matches, I call `IMatchingService` which calculates a confidence score (0.0 to 1.0) across secondary attributes:
    * **Normalized Business Name:** Jaro-Winkler similarity on cleaned strings (e.g., ignoring suffix terms like "Ltd", "Limited", "Motors"). Jaro-Winkler handles word additions/suffixes better and works well for short strings and character transpositions.
    * **Postcode:** Exact inward/outward postcode alignment.
    * **Domain Name:** Extracted hostname alignment from URLs.
  * **Threshold:** If the combined confidence score meets or exceeds `MatchingOptions.DedupeConfidenceFloor`, the record joins the highest-scoring bucket. Otherwise, a new cluster is created. 

  Note: The `MatchingOptions.DedupeConfidenceFloor` is held in appSettings bound to amodel class `MatchingOptions` via `IOptions<MatchingOptions>` and can be tuned per run. 
---

## 2. Source Conflict Resolution Strategy

When multiple sources provide conflicting values for the same attribute, I resolve conflicts using **Source Authority Hierarchy** and **Completeness Scoring**:

* **Authority Hierarchy by Field:**
  * **Company Number:** `Companies House` > `FCA Register` > `Crawled Dealers`
  * **FCA FRN:** `FCA Register` > `SAF Members` > `Crawled Dealers`
  * **VAT Number:** `HMRC / VAT Lookup` > `ICO Register` > `Crawled Dealers`
  * **Trading Names & Addresses:** Accumulated as child collections rather than overwritten, maintaining data lineage.

* **Tie-Breaking Rule:** When sources have equal priority, the most complete record (highest count of non-null fields) or the most recent record timestamp wins.
* **Cross-Run Database Matching:** Incoming records are queried against the database using `_dealerRepository.FindExistingDealerIdAsync()`. This provides cross-run deduplication of old and new data.
---

## 3. Key Assumptions Made

1. **File Format Integrity:** Input files (CSV, JSON, XML) broadly match vendor schemas, but readers stream and log skipping on bad rows rather than failing the whole batch.
2. **Stateless Reader Instances:** `IFileDataReader` implementations are registered via Keyed Services and are stateless, allowing safe execution across multiple files.
3. **In-Memory Cluster Size:** The active dataset for a single run fits into memory (`List<DealerBucket>`) prior to committing to Entity Framework Core.
4. **Lineage Auditing:** Storing `RawPayload` (CSV line, JSON element text, XML fragment) on `NormalisedRecord` satisfies any auditing and debugging requirements.

---

## 4. Pipeline Execution & Progress Reporting

The `DataOrchestrator` service acts as the central entry point for processing incoming data and supports flexible modes along with real-time feedback:

* **Execution Entry Points (`ExecutePipelineAsync`):**
  * **Directory Execution:** Accepts a path to a directory containing multiple feed files. The orchestrator scans the folder, resolves file types, orders them by processing priority (`GetFileProcessingPriority()`), and executes the pipeline sequentially across all files.
  * **Single File Execution:** Accepts a path directly to a specific file. Useful for target processing, testing, or isolated re-runs without scanning entire directories.

* **Progress Reporting (`IProgress<PipelineProgressReport>`):**
  * The execution methods accept an optional `IProgress<PipelineProgressReport>` parameter to stream real-time updates back to callers (console app UI, progress bars, or web workers).
  * **Metrics Tracked:** Reports current file name, files completed vs. total files, total records read, records merged into existing buckets, new buckets created, and total persisted database records.
  * **Non-Blocking Feedback:** Progress is reported asynchronously during ingestion stages, enabling real-time UI updates without interrupting reader or database operations.

---

## 5. Scope Trade-Offs & Future Enhancements

### What I Chose Not to Do

* **Async Reader Streaming:** File readers currently yield synchronously (`IEnumerable<NormalisedRecord>`).
* **Progress Reporting:** While the orchestrator reports progress, I have not implemented this function in the Console App in  `DealerDatabase.Import`. Add the ability to call the Import Process in the Web version, A progress bar and details would have been available to show the User that something is happening. 
* **Web Application:** I focused on the Console App to Import the Data. The only changes made to the Web View are purely for my benefit to check the data to find out the causes to unwanted duplication. 

## 6. New Files to Process
 1. Add a new class implimenting `IFileDataReader` for the file format (e.g., `XmlFileDataReader`, `CsvFileDataReader`, `JsonFileDataReader`). This Reader file will load the File and read the data and build a `NormalisedRecord` object.
 2. For the Dependency Injection, add the new KeyedTransient call to `DataReaderConfiguration` assigning a Key to distinguish the newly written `IFileDataReader` class.
 3. Under the correct file extension section, add the new call to return the new `IFileDataReader` class. Using data in the first line of the file to recongise the file contents and return the correct `IFileDataReader` class.
 4. If you wish to set a Priority for the new file format, this can be done adding to the `GetFileProcessingPriority()` method in `DataOrchestration` class. This will allow the new file format to be processed before or after other file formats.

### What We Would Do Next (With More Time)

* **Blocking / Indexing for Fuzzy Matching:** Introduce spatial or prefix blocking (e.g., group candidates by Outward Postcode or Name Prefix) before running O(N) fuzzy comparisons to scale to hundreds of thousands of records.
* **Async Enumeration (`IAsyncEnumerable<T>`):** Refactor `IFileDataReader` and `IngestionService` to stream large JSON/CSV feeds asynchronously to reduce memory overhead.
* **DLQ (Dead Letter Queue):** Enhance `RecordFailedRecords()` to dump failed DB payload instances into a dedicated DLQ database table or S3 bucket for automatic reprocessing.