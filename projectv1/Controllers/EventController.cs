using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projectv2.Data;
using projectv2.Models;
using System;

namespace projectv2.Controllers
{
    public class EventController : BaseController
    {
        // GET: UserController
        private readonly ApplicationDbContext dbContext;
        public EventController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var userId = int.Parse(HttpContext.Session.GetString("UserId"));
            var events = await dbContext.Events
                .Where(e => e.OrganizerId == userId)
                .Include(e => e.EventHasFriends) // Include related EventHasFriends
                    .ThenInclude(e => e.User)
                .Include(e => e.Organizer)
                                 .ToListAsync();
			    return View(events);
        }

        // GET: UserController/Create
        public async Task<ActionResult> Create()
        {
            return View();
        }

        // POST: UserController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(EventViewModel viewModel)
        {

            var events = new Event
            {
                Name = viewModel.Name,
                Date = viewModel.Date,
                Location = viewModel.Location,
				Description = viewModel.Description,
                OrganizerId = int.Parse(HttpContext.Session.GetString("UserId")),
                Status = viewModel.Status,
			};

            await dbContext.Events.AddAsync(events);
            await dbContext.SaveChangesAsync();

            // Loop through the FriendIds and create a new EventHasFriend for each friend
            /*var eventHasFriends = new List<EventHasFriend>();
            foreach (var friendId in viewModel.FriendIds)
            {
                var eventHasFriend = new EventHasFriend
                {
                    EventId = events.Id, // Use the EventId from the saved event
                    FriendId = friendId // Use the current friend's ID from the loop
                };

                eventHasFriends.Add(eventHasFriend); // Add to the list
            }

            // Add all the EventHasFriends to the database
            await dbContext.EventHasFriends.AddRangeAsync(eventHasFriends);
            await dbContext.SaveChangesAsync(); // Save the changes*/

            return RedirectToAction(nameof(Index));
        }

        // GET: UserController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var userId = int.Parse(HttpContext.Session.GetString("UserId"));
            var existingEvent = await dbContext.Events.FindAsync(id);

            if (existingEvent == null || existingEvent.OrganizerId != userId)
            {
                return BadRequest("You can only edit your own events.");
            }
            /*var events = await dbContext.Events
                .Include(e => e.EventHasFriends) // Include the related EventHasFriends table
                .ThenInclude(ehf => ehf.Friend) // Optionally, include the Friend data as well (for more details)
                .FirstOrDefaultAsync(e => e.Id == id);*/



            //var friendIds = events.EventHasFriends.Select(ehf => ehf.FriendId).ToList(); // Get the list of FriendIds

            //var friends = await dbContext.Friends.ToListAsync(); // Retrieve all friends

            var model = new EventViewModel
            {
                Id = existingEvent.Id,
                Name = existingEvent.Name,
                Date = existingEvent.Date,
                Location = existingEvent.Location,
                Description = existingEvent.Description,
                OrganizerId = existingEvent.OrganizerId,
                Status = existingEvent.Status,
            };

            return View(model);
        }

        // POST: UserController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(EventViewModel viewModel)
		{

            var userId = int.Parse(HttpContext.Session.GetString("UserId"));
            var eventToUpdate = await dbContext.Events.Include(e => e.EventHasFriends)
                                                      .FirstOrDefaultAsync(e => e.Id == viewModel.Id);

            if (eventToUpdate == null || eventToUpdate.OrganizerId != userId)
            {
                return BadRequest("You can only edit your own events.");
            }

			eventToUpdate.Name = viewModel.Name;
			eventToUpdate.Date = viewModel.Date;
			eventToUpdate.Location = viewModel.Location;
			eventToUpdate.Description = viewModel.Description;
            eventToUpdate.Status = viewModel.Status;

			await dbContext.SaveChangesAsync();

			return RedirectToAction("Index", "Event");
		}

        // POST: UserController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(EventViewModel viewModel)
        {
            var events = await dbContext.Events.FindAsync(viewModel.Id);

            if (events != null)
            {
                dbContext.Events.Remove(events);
                await dbContext.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Event");
        }

        [HttpPost]
        public async Task<IActionResult> Join(int eventId)
        {
            var userId = int.Parse(HttpContext.Session.GetString("UserId"));

            // Check if already joined
            var existingEntry = await dbContext.EventHasFriends
                .FirstOrDefaultAsync(ehf => ehf.EventId == eventId && ehf.UserId == userId);

            if (existingEntry != null)
            {
                return BadRequest("You are already part of this event.");
            }

            var eventHasFriend = new EventHasFriend
            {
                EventId = eventId,
                UserId = userId
            };

            dbContext.EventHasFriends.Add(eventHasFriend);
            await dbContext.SaveChangesAsync();
            return RedirectToAction("JoinEvent", "Event");
        }

        [HttpGet]
        // GET: Event/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Retrieve the event with the given id, including related data if needed (e.g., users attending)
            var eventDetails = await dbContext.Events
                .Include(e => e.EventHasFriends) // Include the related EventHasFriend entries
                    .ThenInclude(ehf => ehf.User) // Include the User who is attending
                .Include(e => e.Organizer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (eventDetails == null)
            {
                return NotFound();
            }

            return View(eventDetails);
        }

        [HttpGet]
        public async Task<IActionResult> JoinEvent()
        {
            var userId = int.Parse(HttpContext.Session.GetString("UserId"));

            // Get the list of friends' IDs for the current user
            var friendIds = await dbContext.Kawans
                .Where(k => k.UserId == userId)
                .Select(k => k.FriendId)
                .ToListAsync();

            // Get events organized by friends
            var events = await dbContext.Events
                .Where(e => friendIds.Contains(e.OrganizerId))
                .Include(e => e.Organizer) // Include Organizer details
                .Include(e => e.EventHasFriends)
                .ToListAsync();

            return View(events);
        }

        public async Task<IActionResult> QuitEvent(int eventId)
        {
            // Assuming you are using the current logged-in user
            var userId = int.Parse(HttpContext.Session.GetString("UserId"));  // Replace with your actual method to get the current user's ID

            // Find the relationship between the user and the event
            var eventHasFriend = await dbContext.EventHasFriends
                .FirstOrDefaultAsync(ehf => ehf.EventId == eventId && ehf.UserId == userId);

            // If the user is not attending the event, return a not found or error message
            if (eventHasFriend == null)
            {
                return NotFound("You are not attending this event.");
            }

            // Remove the relationship
            dbContext.EventHasFriends.Remove(eventHasFriend);

            // Save changes to the database
            await dbContext.SaveChangesAsync();

            // Optionally, you can redirect to the event details page or any other page
            return RedirectToAction("JoinEvent", "Event");
        }

    }
}
