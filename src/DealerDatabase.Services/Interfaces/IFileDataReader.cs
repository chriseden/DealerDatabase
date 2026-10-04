using DealerDatabase.Models;


namespace DealerDatabase.Services.Interfaces
{
    public interface IFileDataReader
    {
        IEnumerable<NormalisedRecord> ReadFile(string filePath);
    }
}
