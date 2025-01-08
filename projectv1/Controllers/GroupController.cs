using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projectv2.Data;
using projectv2.Models;
using System;

namespace projectv2.Controllers
{
    public class GroupController : BaseController
    {
        // GET: UserController
        private readonly ApplicationDbContext dbContext;
        public GroupController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            // Get the UserId from session
            var userId = int.Parse(HttpContext.Session.GetString("UserId"));

            // Get the list of groups that belong to the user
            var groupsWithFriendCounts = await dbContext.Groups
                .Where(g => g.UserId == userId)
                .Select(group => new
                {
                    Group = group,
                    FriendCount = dbContext.Kawans.Count(k => k.GroupId == group.Id) // Count the friends in the group
                })
                .ToListAsync();

            // Pass the groups along with their friend count to the view
            return View(groupsWithFriendCounts);
        }


        // GET: UserController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: UserController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(GroupViewModel viewModel)
        {
            var userId = int.Parse(HttpContext.Session.GetString("UserId"));
            var group = new Group
            {
                Name = viewModel.Name,
                Description = viewModel.Description,
                UserId = userId
            };

            await dbContext.Groups.AddAsync(group);
            await dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: UserController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var group = await dbContext.Groups.FindAsync(id);

            if (group  == null)
            {
                return NotFound();
            }

            return View(new GroupViewModel
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description
            });
        }

        // POST: UserController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GroupViewModel viewModel)
        {

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var group = await dbContext.Groups.FindAsync(viewModel.Id);

            if (group == null)
            {
                return NotFound();
            }

            group.Name = viewModel.Name;
            group.Description = viewModel.Description;
            await dbContext.SaveChangesAsync();

            return RedirectToAction("Index", "Group");
        }

        // GET: UserController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: UserController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(GroupViewModel viewModel)
        {
            var group = await dbContext.Groups.FindAsync(viewModel.Id);

            if (group != null)
            {
                dbContext.Groups.Remove(group);
                await dbContext.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Group");
        }
    }
}
