using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projectv2.Data;
using projectv2.Models;
using System;
using System.Security.Claims;

namespace projectv2.Controllers
{
    public class KawanController : BaseController
    {
        private readonly ApplicationDbContext dbContext;
        public KawanController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        // GET: Friend/Index
        public async Task<IActionResult> Index()
        {
            // Get the current user's ID
            var userId = int.Parse(HttpContext.Session.GetString("UserId"));

            // Get the friends where the current user is either the UserId or the FriendId
            var friends = await dbContext.Kawans
                .Include(k => k.User)   // Include the User entity
                .Include(k => k.Friend) // Include the Friend 
                .Include(k => k.Groups)
                .Where(f => f.UserId == userId)
                .ToListAsync();

            // Pass the friends to the view
            return View(friends);
        }

        // GET: UserController/Create
        public async Task<IActionResult> Create()
        {
            // Get the current user's ID from session
            var userId = int.Parse(HttpContext.Session.GetString("UserId"));

            // Get the list of users excluding the current user
            // Also exclude users who are already friends with the current user
            var users = await dbContext.Users
                .Where(u => u.Id != userId &&
                            !dbContext.Kawans.Any(f => (f.UserId == userId && f.FriendId == u.Id) || (f.UserId == u.Id && f.FriendId == userId)))
                .ToListAsync();

            // Get the friend requests where the current user is involved
            var friendRequests = await dbContext.FriendRequests
                .Where(fr => (fr.SenderId == userId && fr.IsRejected == false ))
                .ToListAsync();

            // Pass the list of users and the friend requests to the view
            var viewModel = users.Select(user => new FriendRequestViewModel
            {
                User = user,
                IsRequestSent = friendRequests.Any(fr => fr.ReceiverId == user.Id)
            }).ToList();

            return View(viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> UnFriend(int friendId)
        {
            // Get the current user's ID from the session
            var userId = int.Parse(HttpContext.Session.GetString("UserId"));

            // Find the Kawan relationship where either UserId or FriendId matches the current user and the other user
            var friendRelations = await dbContext.Kawans
                .Where(k => (k.UserId == userId && k.FriendId == friendId) ||
                            (k.UserId == friendId && k.FriendId == userId))
                .ToListAsync();

            if (friendRelations == null)
            {
                return BadRequest("Friendship not found.");
            }

            // Remove the friendship record
            dbContext.Kawans.RemoveRange(friendRelations);
            await dbContext.SaveChangesAsync();

            return RedirectToAction("Index", "Kawan"); // Redirect back to the friends list
        }

        // GET: Kawan/Edit/5
        public async Task<IActionResult> Edit(int friendId)
        {
            var friend = await dbContext.Kawans
                .Include(k => k.Friend)
                .Include(k => k.Groups)
                .FirstOrDefaultAsync(k => k.FriendId == friendId);

            if (friend == null)
            {
                return NotFound();
            }

            // Get the UserId from the session
            var userId = int.Parse(HttpContext.Session.GetString("UserId"));

            // Filter groups based on the UserId matching the session's UserId
            var groups = await dbContext.Groups
                .Where(g => g.UserId == userId)  // Only groups where UserId matches session UserId
                .ToListAsync();

            var viewModel = new KawanViewModel
            {
                GroupId = friend.GroupId, // Assuming friend has a GroupId field
                Groups = groups, // Use the filtered groups
                Name = friend.Friend?.Name
            };

            return View(viewModel);
        }

        // POST: Kawan/Edit/5
        // POST: Kawan/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(KawanViewModel model)
        {
            // Get the UserId from the session
            var userId = int.Parse(HttpContext.Session.GetString("UserId"));

            // Check if the GroupId exists in the Groups table
            var groupExists = await dbContext.Groups
                    .AnyAsync(g => g.Id == model.GroupId);

            if (!groupExists)
            {
                // Return an error if the group does not exist
                ModelState.AddModelError("GroupId", "The selected group does not exist.");
                model.Groups = await dbContext.Groups.ToListAsync(); // Reload groups
                return RedirectToAction(nameof(Index));
            }

            // Retrieve the friend record to update
            var friend = await dbContext.Kawans
                .FirstOrDefaultAsync(k => k.FriendId == model.FriendId && k.UserId == userId);

            if (friend == null)
            {
                return NotFound();
            }

            // Update the group for the friend
            friend.GroupId = model.GroupId;

            // Save changes to the database
            await dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index)); // Redirect back to the index or any other page
        }

    }
}
