using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projectv2.Data;
using projectv2.Models;
using System.Diagnostics;

namespace projectv2.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context; // Replace with your DbContext
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            this._context= context;
        }

        public async Task<IActionResult> Index()
        {
            // Get the logged-in user's ID from session
            var userId = int.Parse(HttpContext.Session.GetString("UserId"));
            var user = await _context.Users.FindAsync(userId);

            // Pass the friend count to the view
            ViewData["UserNotes"] = user.Notes;

            if (userId == null)
            {
                // If no user is logged in, redirect to login page
                return RedirectToAction("Login", "User");
            }

            // Fetch the count of friends for the logged-in user
            var friendCount = GetFriendCount(userId);

            // Pass the friend count to the view
            ViewData["FriendCount"] = friendCount;

            // Get the number of groups
            var groupCount = await _context.Groups.CountAsync(g => g.UserId == userId);
            ViewData["GroupCount"] = groupCount;

            // Fetch recent friend requests where the current user is the receiver
            var recentFriendRequests = await _context.FriendRequests
                .Where(fr => fr.ReceiverId == userId && fr.IsRejected == false && fr.IsAccepted == false)
                .Include(fr => fr.Sender)
                .ToListAsync();

            // Pass data to the view
            ViewBag.RecentFriendRequests = recentFriendRequests;

            // Fetch events created by friends
            var friendEvents = await _context.Events
                .Where(e => _context.Kawans
                    .Where(k => (k.UserId == userId && k.FriendId == e.OrganizerId) ||
                                (k.FriendId == userId && k.UserId == e.OrganizerId))
                    .Any())
                .Include(e => e.Organizer)
                .OrderByDescending(e => e.Date)
                .Take(5) // Fetch up to 5 recent events
                .ToListAsync();

            ViewBag.FriendEvents = friendEvents;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Method to get the count of friends for the given user
        private int GetFriendCount(int userId)
        {
            // Assuming a many-to-many relationship between users and friends
            // and that you have a Friend table that connects users.

            var friendCount = _context.Kawans
                .Where(f => f.UserId == userId)  // Find all friends of the user
                .Count();

            return friendCount;
        }
    }
}