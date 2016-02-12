using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InzWebApi.Models
{
    public class Information
    {
        public long Id { get; set; }    //nr_informacji

        [ForeignKey("Traveler")]
        public long TravelerId { get; set; }    //numer właściciela
        [Required]
        public DateTime Time { get; set; }  //czas
        [Required]
        public string StartPlace { get; set; }  //miejscowość początkowa
        [Required]
        public string EndPlace { get; set; }    //miejscowość końcowa
        
        public virtual Traveler Traveler { get; set; }  //użytkownik, który dodał informację
        
    }
}
