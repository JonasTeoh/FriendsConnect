using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projectv2.Data;
using projectv2.Models;
using System;

namespace projectv2.Controllers
{
    public class FriendController : BaseController
    {
        // GET: UserController
        private readonly ApplicationDbContext dbContext;
        public FriendController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var friends = await dbContext.Friends.ToListAsync();
            return View(friends);
        }

        // GET: UserController/Create
        public async Task<ActionResult> Create()
        {
            var groups = await dbContext.Groups.ToListAsync(); // Retrieve all friends
            var model = new FriendViewModel
            {
                Groups = groups // Pass the list of friends to the view model
            };
            return View(model);
        }

        // POST: UserController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(FriendViewModel viewModel)
        {

            var friends = new Friend
            {
                Name = viewModel.Name,
                ContactInfo = viewModel.ContactInfo,
                Birthday = viewModel.Birthday,
                Notes = viewModel.Notes,
                GroupId = viewModel.GroupId,
            };

            await dbContext.Friends.AddAsync(friends);
            await dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: UserController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var friend = await dbContext.Friends.FindAsync(id);

            if (friend == null)
            {
                return NotFound();
            }

            var groups = await dbContext.Groups.ToListAsync(); // Retrieve all friends

            return View(new FriendViewModel
            {
                Id = friend.Id,
                Name = friend.Name,
                ContactInfo = friend.ContactInfo,
                Birthday = friend.Birthday,
                Notes = friend.Notes,
                GroupId = friend.GroupId,
                Groups = groups // Pass the list of friends to the view model
            });
        }

        // POST: UserController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(FriendViewModel viewModel)
        {

            var friend = await dbContext.Friends.FindAsync(viewModel.Id);

            if (friend == null)
            {
                return NotFound();
            }

            friend.Name = viewModel.Name;
            friend.ContactInfo = viewModel.ContactInfo;
            friend.Birthday = viewModel.Birthday;
            friend.Notes = viewModel.Notes;
            friend.GroupId = viewModel.GroupId;
            await dbContext.SaveChangesAsync();

            return RedirectToAction("Index", "Friend");
        }

        // GET: UserController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: UserController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(FriendViewModel viewModel)
        {
            var friend = await dbContext.Friends.FindAsync(viewModel.Id);

            if (friend != null)
            {
                dbContext.Friends.Remove(friend);
                await dbContext.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Friend");
        }
    }
}
