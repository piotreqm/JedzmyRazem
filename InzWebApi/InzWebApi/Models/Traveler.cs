using System;
using System.ComponentModel.DataAnnotations;

namespace InzWebApi.Models
{
    public class Traveler
    {
        public long Id { get; set; }    //Id
        [Required]
        public string Name { get; set; }    //Imię
        [Required]
        public string Surname { get; set; } //Nazwisko
        [Required]
        public DateTime DateOfBirth { get; set; }    //Wiek

        public string UserId { get; set; }  //id konta użytkownika podróżującego
    }
}
