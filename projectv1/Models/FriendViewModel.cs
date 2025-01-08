namespace projectv2.Models
{
	public class FriendViewModel
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string ContactInfo { get; set; }
		public DateTime Birthday { get; set; }
		public string? Notes { get; set; }
		public int GroupId { get; set; }
		public ICollection<Group> Groups { get; set; }
	}
}
