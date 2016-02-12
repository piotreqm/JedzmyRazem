using System;
using System.Collections.Generic;
using System.Linq;

namespace InzWebApi.Models
{
    internal class TravelerRepository : ITravelerRepository
    {
        ApplicationDbContext context = new ApplicationDbContext();
        private List<Traveler> travelers = new List<Traveler>();

        public TravelerRepository()
        {
            RefreshTravelerList();
        }

        private void RefreshTravelerList()
        {
            travelers = (from p in context.Travelers select p).OrderBy(p => p.Surname).ToList();
        }

        public IEnumerable<Traveler> GetAll()
        {
            return travelers;
        }

        public List<Traveler> Get(long travelerId)
        {
            return travelers.FindAll(p => p.Id == travelerId);
        }

        public List<Traveler> GetByName(string travelerName, string travelerSurname)
        {
            return travelers.FindAll(p => p.Name == travelerName && p.Surname == travelerSurname).ToList();
        }

        public Traveler GetByUserId(string userId)
        {
            return travelers.Find(p => p.UserId == userId);
        }

        public Traveler Add(Traveler item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            else
            {
                context.Travelers.Add(item);
                context.SaveChanges();
                RefreshTravelerList();
                return item;
            }
        }

        public bool Remove(long travelerId)
        {
            try
            {
                var travelerToRemove = (from p in context.Travelers where p.Id == travelerId select p).First();
                context.Travelers.Remove(travelerToRemove);
                context.SaveChanges();
                RefreshTravelerList();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
