using InzWebApi.Models;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace InzWebApi.Controllers
{
    public class TravelerController : ApiController
    {
        static readonly ITravelerRepository repo = new TravelerRepository();
        private List<Traveler> Travelers { get; set; }
        private Traveler Traveler { get; set; }

        //GET api/traveler/GetAll
        [Authorize]
        public IEnumerable<Traveler> GetAll()
        {
            return repo.GetAll();
        }

        //GET api/traveler/GetTraveler?travelerId=1
        [Authorize]
        public List<Traveler> GetTraveler(long travelerId)
        {
            Travelers = repo.Get(travelerId);
            if (Travelers.Count == 0) 
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            else return Travelers;
        }

        //GET api/traveler/GetTravelerByUserId?travelerName=name
        [Authorize]
        public Traveler GetTravelerByUserId(string userId)
        {
            Traveler = repo.GetByUserId(userId);
            if (Traveler == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return Traveler;
        }

        //GET api/traveler/GetTravelerByName?travelerName=name&travelerSurname=surname
        [Authorize]
        public List<Traveler> GetTravelerByName(string travelerName, string travelerSurname)
        {
            Travelers = repo.GetByName(travelerName, travelerSurname);
            if (Travelers.Count == 0) 
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            else return Travelers;
        }

        //POST api/traveler/postTraveler
        [AllowAnonymous] //because it's used to register that allows too
        public HttpResponseMessage PostTraveler(Traveler newTraveler)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
            else
            {
                newTraveler = repo.Add(newTraveler);
                var response = Request.CreateResponse(HttpStatusCode.Created, newTraveler);
                return response;
            } 
        }

        //DELETE api/traveler/deleteTraveler
        [Authorize]
        public void DeleteTraveler(long travelerToDeleteId)
        {
            var removeMessage = repo.Remove(travelerToDeleteId);
            if (!removeMessage)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }
    }
}
