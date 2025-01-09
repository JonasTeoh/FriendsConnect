using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using projectv2.Data;
using projectv2.Models;
using System;

namespace projectv2.Controllers
{
    public class UserController : Controller
    {
        // GET: UserController
        private readonly ApplicationDbContext dbContext;
        public UserController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            if (HttpContext.Session.GetString("UserId") != "1")
            {
                return RedirectToAction("Login", "User");
            }
            var users = await dbContext.Users.ToListAsync();
            return View(users);
        }

        [HttpGet]
        // GET: UserController/Create
        public ActionResult Create()
        {
            if (HttpContext.Session.GetString("UserId") != "1")
            {
                return RedirectToAction("Login", "User");
            }
            return View();
        }

        // POST: UserController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(UserViewModel viewModel)
        {
            if (HttpContext.Session.GetString("UserId") != "1")
            {
                return RedirectToAction("Login", "User");
            }
            if (await dbContext.Users.AnyAsync(u => u.Email == viewModel.Email))
            {
                ModelState.AddModelError("Email", "Email already exists.");
                return View(viewModel);
            }
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(viewModel.Password); // Hash the password
            var users = new User
            {
                Name = viewModel.Name,
                Email = viewModel.Email,
                PasswordHash = hashedPassword,
                ContactInfo = viewModel.ContactInfo,
                Birthday = viewModel.Birthday,
                Notes = viewModel.Notes
            };

            await dbContext.Users.AddAsync(users);
            await dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: UserController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            if (HttpContext.Session.GetString("UserId") != "1")
            {
                if (int.Parse(HttpContext.Session.GetString("UserId")) != id)
                {
                    return RedirectToAction("Login", "User");
                }
            }
            var user = await dbContext.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return View(new UserViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                ContactInfo = user.ContactInfo,
                Birthday = user.Birthday,
                Notes = user.Notes
            });
        }

        // POST: UserController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserViewModel viewModel)
        {
            if (HttpContext.Session.GetString("UserId") != "1")
            {
                if (int.Parse(HttpContext.Session.GetString("UserId")) != viewModel.Id)
                {
                    return RedirectToAction("Login", "User");
                }
            }
            var existingUser = await dbContext.Users
                .Where(u => u.Id != viewModel.Id)
                .AnyAsync(u => u.Email == viewModel.Email);
            if (existingUser)
            {
                ModelState.AddModelError("Email", "Email already exists.");
                return View(viewModel);
            }

            var user = await dbContext.Users.FindAsync(viewModel.Id);

            if (user == null)
            {
                return NotFound();
            }

            user.Name = viewModel.Name;
            user.Email = viewModel.Email;
            user.Email = viewModel.ContactInfo;
            user.Birthday = viewModel.Birthday;
            user.Notes = viewModel.Notes;
            await dbContext.SaveChangesAsync();
            HttpContext.Session.SetString("UserName", viewModel.Name);

            return RedirectToAction("Index", "User");
        }

        // POST: UserController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(UserViewModel viewModel)
        {
            if (HttpContext.Session.GetString("UserId") != "1")
            {
                if (int.Parse(HttpContext.Session.GetString("UserId")) != viewModel.Id)
                {
                    return RedirectToAction("Login", "User");
                }
            }

            // Delete related Kawan records
            var kawans = await dbContext.Kawans
                .Where(k => k.FriendId == viewModel.Id)
                .ToListAsync();

            if (kawans != null && kawans.Any())
            {
                dbContext.Kawans.RemoveRange(kawans);
            }

            // Delete EventHasFriend records related to the User
            var user = await dbContext.Users
                .Include(u => u.EventHasFriends) // Include related EventHasFriends
                .FirstOrDefaultAsync(u => u.Id == viewModel.Id);

            if (user != null)
            {
                // Check if EventHasFriends is not null or empty before attempting to remove
                if (user.EventHasFriends != null)
                {
                    dbContext.EventHasFriends.RemoveRange(user.EventHasFriends);
                }
                // Delete FriendRequests where the user is either the sender or receiver
                var friendRequests = await dbContext.FriendRequests
                    .Where(fr => fr.SenderId == user.Id || fr.ReceiverId == user.Id)
                    .ToListAsync();

                if (friendRequests != null && friendRequests.Any())
                {
                    dbContext.FriendRequests.RemoveRange(friendRequests);
                }

                // Now delete the user itself
                dbContext.Users.Remove(user);

                await dbContext.SaveChangesAsync();
            }

            if (int.Parse(HttpContext.Session.GetString("UserId")) == viewModel.Id)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "User");
            }
            return RedirectToAction("Index", "User");
        }

        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("UserId") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel viewModel)
        {
            if (HttpContext.Session.GetString("UserId") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == viewModel.Email);

            if (user != null && BCrypt.Net.BCrypt.Verify(viewModel.Password, user.PasswordHash))
            {
                // Authentication successful
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("UserName", user.Name);
                return RedirectToAction("Index", "Home");
            }

            // Authentication failed
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(viewModel);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "User");
        }

        // GET: UserController/Create
        public ActionResult Register()
        {
            if (HttpContext.Session.GetString("UserId") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: UserController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(UserViewModel viewModel)
        {
            if (HttpContext.Session.GetString("UserId") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            if (await dbContext.Users.AnyAsync(u => u.Email == viewModel.Email))
            {
                ModelState.AddModelError("Email", "Email already exists.");
                return View(viewModel);
            }
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(viewModel.Password); // Hash the password
            var users = new User
            {
                Name = viewModel.Name,
                Email = viewModel.Email,
                PasswordHash = hashedPassword,
                ContactInfo = viewModel.ContactInfo,
				Birthday = viewModel.Birthday,
				Notes = viewModel.Notes
            };

            await dbContext.Users.AddAsync(users);
            await dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Login));
        }

        // Details Action
        public async Task<IActionResult> Details(int id)
        {
            var user = await dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
    }
}
