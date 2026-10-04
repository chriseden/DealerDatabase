namespace DealerDatabase.Services.Interfaces
{
    public interface IDynamicReaderFactory
    {
        IEnumerable<IFileDataReader> GetReader(string path);
    }
}