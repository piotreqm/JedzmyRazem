using System.Collections.Generic;

namespace InzWebApi.Models
{
    public interface ITravelerRepository
    {
        IEnumerable<Traveler> GetAll();
        List<Traveler> Get(long travelerId);
        List<Traveler> GetByName(string travelerName, string travelerSurname);
        Traveler GetByUserId(string userId);
        Traveler Add(Traveler item);
        bool Remove(long travelerId);
    }
}
