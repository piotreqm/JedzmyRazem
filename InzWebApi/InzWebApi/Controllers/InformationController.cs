using InzWebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace InzWebApi.Controllers
{
    public class InformationController : ApiController
    {   //implementacja repozytorium
        static readonly IInformationRepository repo = new InformationRepository();
        private List<Information> Informations { get; set; }

        //GET api/information/getinformation
        [Authorize]
        public IEnumerable<Information> GetInformation()
        {
            return repo.GetAll();
        }

        //GET api/information/getinformation?travelerId=1
        [Authorize]
        public List<Information> GetInformation(long travelerId)
        {
            Informations = repo.Get(travelerId);
            if (Informations.Count == 0) 
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            else return Informations;
        }

        //GET api/information/getOtherTravelersByInformations?travelHour=10&travelMinutes=30&startPlace=start&endPlace=end
        [Authorize]
        public List<Traveler> GetOtherTravelersByInformations(int travelHour, int travelMinutes, string startPlace, string endPlace)
        {
            var travelTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, travelHour, travelMinutes, 0);
            Informations = repo.GetOtherTravelersInformations(travelTime, startPlace, endPlace);
            if (Informations.Count == 1 && Informations[0] == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            var otherTravelersFoundByInformations = new List<Traveler>();
            foreach (var information in Informations)
                otherTravelersFoundByInformations.Add(information.Traveler);
            return otherTravelersFoundByInformations;
        }

        //POST api/information/postinformation
        [Authorize]
        public HttpResponseMessage PostInformation(Information newMessage)
        {
            if (!ModelState.IsValid)
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            newMessage = repo.Add(newMessage);
            var response = Request.CreateResponse(HttpStatusCode.Created, newMessage);
            return response;
        }

        //DELETE api/information/deleteinformation?messageToDeleteId=id
        [Authorize]
        public void DeleteInformation(long messageToDeleteId)
        {
            var removeMessage = repo.Remove(messageToDeleteId);
            if (!removeMessage)
                throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        //DELETE api/information/deleteagedinformations
        [AllowAnonymous]
        public HttpResponseMessage DeleteAgedInformations()
        {
            if (Request.Headers.GetValues("requestAuth").FirstOrDefault() != "InzWebApiScheduler")
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            var removeAgedMessages = repo.RemoveAged();
            return removeAgedMessages ? new HttpResponseMessage(HttpStatusCode.OK) : new HttpResponseMessage(HttpStatusCode.NotFound);
        }
    }
}
