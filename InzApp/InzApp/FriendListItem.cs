namespace InzApp
{
    internal class FriendListItem
    {
        public long Id { get; set; }
        public long OwnerId { get; set; }
        public long FriendId { get; set; }
        public virtual Traveler Owner { get; set; }
        public virtual Traveler Friend { get; set; }
    }
}