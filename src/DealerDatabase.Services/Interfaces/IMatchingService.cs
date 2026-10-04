using DealerDatabase.Models;

namespace DealerDatabase.Services.Interfaces
{
    public interface IMatchingService
    {     
        double CalculateMatchScore(NormalisedRecord recordA, NormalisedRecord recordB);
        string CleanPostcode(string postCode);
    }
}