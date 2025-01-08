namespace projectv2.Models
{
    public class Event
    {
        public int Id { get; set; } 
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public int OrganizerId { get; set; } // User ID of the organizer
        public string Status { get; set; }
        public User? Organizer { get; set; }
        // Add this navigation property
        public ICollection<EventHasFriend> EventHasFriends { get; set; }
	}
}
