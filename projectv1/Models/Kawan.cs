namespace projectv2.Models
{
    public class Kawan
    {
        public int Id { get; set; }

        // The User who is sending the friend request
        public int UserId { get; set; }
        public User User { get; set; }

        // The User who is receiving the friend request
        public int FriendId { get; set; }
        public int? GroupId { get; set; }
        public User Friend { get; set; }
        public Group Groups { get; set; }
    }
}
