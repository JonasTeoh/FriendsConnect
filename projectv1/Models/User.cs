namespace projectv2.Models
{
    public class User
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; } // For login credentials
        public string PasswordHash { get; set; } // Store a hashed password
        public string ContactInfo { get; set; }
        public DateTime Birthday { get; set; }
        public string? Notes { get; set; }
        public EventHasFriend EventHasFriends { get; set; }
    }
}