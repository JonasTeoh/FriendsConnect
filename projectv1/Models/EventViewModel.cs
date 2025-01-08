namespace projectv2.Models
{
    public class EventViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public int OrganizerId { get; set; } // User ID of the organizer
        public string Status { get; set; }
        public User Organizer { get; set; }
        // Store multiple Friend IDs
        public List<int> FriendIds { get; set; } = new List<int>();
        // This will hold the list of friends to display in the checkboxes
        public List<Kawan> Friends { get; set; }
    }
}
