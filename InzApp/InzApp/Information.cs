using System;

namespace InzApp
{
    internal class Information
    {
        public long Id { get; set; }
        public long TravelerId { get; set; }
        public DateTime Time { get; set; }
        public string StartPlace { get; set; }
        public string EndPlace { get; set; }
        public Traveler Traveler { get; set; }
    }
}