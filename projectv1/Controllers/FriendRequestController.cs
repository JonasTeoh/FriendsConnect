using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projectv2.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projectv2.Data;
using projectv2.Models;
using System;
using System.Security.Claims;

namespace projectv2.Controllers
{
    public class FriendRequestController : BaseController
    {
        // GET: UserController
        private readonly ApplicationDbContext dbContext;
        public FriendRequestController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<IActionResult> Index()
        {
            // Get the current user's ID
            var userId = int.Parse(HttpContext.Session.GetString("UserId"));
            // Get the pending friend requests where the current user is the receiver
            var pendingRequests = await dbContext.FriendRequests
                .Where(fr => fr.ReceiverId == userId && fr.IsRejected == false && fr.IsAccepted == false)
                .Include(fr => fr.Sender) // Include sender details
                .ToListAsync();

            // Pass the pending requests to the view
            return View(pendingRequests);
        }

        [HttpPost]
        public async Task<IActionResult> AcceptRequest(int requestId)
        {
            var friendRequest = await dbContext.FriendRequests
                .Include(fr => fr.Sender)
                .Include(fr => fr.Receiver)
                .FirstOrDefaultAsync(fr => fr.Id == requestId);
            var userId = int.Parse(HttpContext.Session.GetString("UserId"));
            if(friendRequest == null)
            {
                return BadRequest("No Friend Request");
            }
            if (friendRequest == null || friendRequest.ReceiverId != userId)
            {
                return BadRequest("Invalid request.");
            }

            friendRequest.IsAccepted = true;
            dbContext.FriendRequests.Update(friendRequest);

            // Add both users to each other's friend list (you will need a 'Friend' table)
            var friend1 = new Kawan { UserId = friendRequest.SenderId, FriendId = friendRequest.ReceiverId };
            var friend2 = new Kawan { UserId = friendRequest.ReceiverId, FriendId = friendRequest.SenderId };

            dbContext.Kawans.Add(friend1);
            dbContext.Kawans.Add(friend2);

            await dbContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> SendRequest(int receiverId)
        {
            var sender = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(HttpContext.Session.GetString("UserId"))); // Get the current user
   
            var receiver = await dbContext.Users.FindAsync(receiverId);
            if (receiver == null)
            {
                return BadRequest("no receiver");
            }
            if (sender == null || receiver == null || sender.Id == receiver.Id)
            {
                return BadRequest("Invalid request.");
            }

            // Check if the friend request already exists
            var existingRequest = await dbContext.FriendRequests
                .FirstOrDefaultAsync(fr => fr.SenderId == sender.Id && fr.ReceiverId == receiver.Id && !fr.IsAccepted && !fr.IsRejected);

            if (existingRequest != null)
            {
                return BadRequest("Friend request already sent.");
            }

            var friendRequest = new FriendRequest
            {
                SenderId = sender.Id,
                ReceiverId = receiver.Id,
                RequestDate = DateTime.Now,
                IsAccepted = false,
                IsRejected = false
            };

            dbContext.FriendRequests.Add(friendRequest);
            await dbContext.SaveChangesAsync();

            return RedirectToAction("Create", "Kawan");
        }

        public async Task<IActionResult> RejectRequest(int requestId)
        {
            var friendRequest = await dbContext.FriendRequests
                .FirstOrDefaultAsync(fr => fr.Id == requestId && fr.ReceiverId == int.Parse(HttpContext.Session.GetString("UserId")));

            if (friendRequest == null)
            {
                return BadRequest("Invalid request.");
            }

            friendRequest.IsRejected = true;
            dbContext.FriendRequests.Update(friendRequest);

            await dbContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }

}
