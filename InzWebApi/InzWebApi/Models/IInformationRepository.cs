using System;
using System.Collections.Generic;

namespace InzWebApi.Models
{
    public interface IInformationRepository
    {   
        IEnumerable<Information> GetAll();
        List<Information> Get(long travelerId);
        List<Information> GetOtherTravelersInformations(DateTime travelTime, string startPlace, string endPlace);
        Information Add(Information item);
        bool Remove(long infomationId);
        bool RemoveAged();
    }
}
