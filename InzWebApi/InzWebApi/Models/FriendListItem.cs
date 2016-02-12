using System.ComponentModel.DataAnnotations.Schema;

namespace InzWebApi.Models
{
    public class FriendListItem
    {
        public long Id { get; set; }    //klucz

        [ForeignKey("Owner")]
        public long OwnerId { get; set; }   //posiadający znajomego
        [ForeignKey("Friend")]
        public long FriendId { get; set; }  //znajomy
        
        public virtual Traveler Owner { get; set; }     //użytkownik-właściciel listy
        
        public virtual Traveler Friend { get; set; }    //użytkownik-znajomy na liście
        
    }    
}
