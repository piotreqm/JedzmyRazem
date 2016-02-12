using System;
using System.Collections.Generic;
using System.Linq;

namespace InzWebApi.Models
{   //klasa implementująca interfejs repozytorium

    internal class InformationRepository : IInformationRepository
    {   //kontekst bazy danych dla aplikacjii repozytorium (lista obiektów)
        ApplicationDbContext context = new ApplicationDbContext();
        private List<Information> informations = new List<Information>();
        
        public InformationRepository()
        {
            RefreshInfomationsList();
        }
        //aktualizacja repozytorium następująca po modyfikacji danych w bazie
        private void RefreshInfomationsList()
        {
            informations = (from p in context.Informations select p).OrderBy(p => p.Time).ToList();
        }
        
        public IEnumerable<Information> GetAll()
        {
            return informations;
        }
        //metoda dla żądania pobrania wiadomości wg id podróżującego
        public List<Information> Get(long travelerId)
        {
            return informations.FindAll(p => p.TravelerId == travelerId);
        }

        public List<Information> GetOtherTravelersInformations(DateTime travelTime, string startPlace, string endPlace)
        {
            return informations.FindAll(p => p.Time == travelTime && p.StartPlace == startPlace && p.EndPlace == endPlace);
        }

        public Information Add(Information item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            context.Informations.Add(item);
            context.SaveChanges();
            RefreshInfomationsList();
            return item;
        }
        
        public bool Remove(long informationId)
        {
            try
            {
                var informationToRemove = (from p in context.Informations where p.Id == informationId select p).First();
                context.Informations.Remove(informationToRemove);
                context.SaveChanges();
                RefreshInfomationsList();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool RemoveAged()
        {
            try
            {
                var plMidnight = DateTime.Now.AddHours(1);
                var agedInformationsToRemove =
                        (from p in context.Informations where DateTime.Compare(p.Time, plMidnight) < 0 select p).ToList();
                context.Informations.RemoveRange(agedInformationsToRemove);
                context.SaveChanges();
                RefreshInfomationsList();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
