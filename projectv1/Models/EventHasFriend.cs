namespace projectv2.Models
{
    public class EventHasFriend
    {
        public int Id { get; set; }

        // Foreign key for Event
        public int EventId { get; set; }
        public Event Event { get; set; } // Navigation property to Event

        // Foreign key for User
        public int? UserId { get; set; }
        public User? User { get; set; } // Navigation property to User
    }
}
