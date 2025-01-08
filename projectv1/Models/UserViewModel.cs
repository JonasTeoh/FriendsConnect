namespace projectv2.Models
{
    public class UserViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string Email { get; set; } // For login credentials
        public string Password { get; set; } // Store a password
        public string ContactInfo { get; set; }
        public DateTime Birthday { get; set; }
        public string? Notes { get; set; }
    }
}
