namespace projectv2.Models
{
    public class FriendRequest
    {
        public int Id { get; set; }
        public int SenderId { get; set; } // The user who sent the friend request
        public int ReceiverId { get; set; } // The user who received the request
        public DateTime RequestDate { get; set; }
        public bool IsAccepted { get; set; } // Whether the request was accepted or not
        public bool IsRejected { get; set; } // Whether the request was rejected or not

        // Navigation properties for relationships
        public User Sender { get; set; }
        public User Receiver { get; set; }
    }
}
