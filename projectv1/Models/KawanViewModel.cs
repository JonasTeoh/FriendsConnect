namespace projectv2.Models
{
    public class KawanViewModel
    {
        public int Id { get; set; }

        // The User who is sending the friend request
        public int UserId { get; set; }
        public User User { get; set; }

        // The User who is receiving the friend request
        public int FriendId { get; set; }
        public string? Name { get; set; }
        public int? GroupId { get; set; }
        public User Friend { get; set; }
        public List<FriendRequest> FriendRequests { get; set; }
        
        public List<Kawan> Kawans { get; set; }
        // List of groups for the select dropdown
        public List<Group> Groups { get; set; }
    }
}
