using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ETSU_Marketplace.ViewModels;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Identity;
using ETSU_Marketplace.Models;
using ETSU_Marketplace.Services;

namespace ETSU_Marketplace.Controllers
{
    /// <summary>
    /// Controller responsible for displaying user profiles and account management.
    /// Allows viewing other users and managing the current user's profile.
    /// </summary>
    public class UsersController : Controller
    {
        // Database context (not heavily used here but available if needed)
        private readonly ApplicationDbContext _db;

        // User manager for authentication and retrieving current user
        private readonly UserManager<ApplicationUser> _userManager;

        // Repository for user-related data operations
        private readonly IUserRepository _userRepo;

        /// <summary>
        /// Constructor that injects dependencies.
        /// </summary>
        /// <param name="db">Application database context</param>
        /// <param name="userManager">ASP.NET Identity user manager</param>
        /// <param name="userRepo">User repository</param>
        public UsersController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            IUserRepository userRepo)
        {
            _db = db;
            _userManager = userManager;
            _userRepo = userRepo;
        }

        /// <summary>
        /// Displays the public profile of a user.
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User profile view</returns>
        public async Task<IActionResult> Details(string id)
        {
            // Retrieve user profile from repository
            var user = await _userRepo.ReadProfileAsync(id);

            if (user == null)
                return NotFound();

            return View(user);
        }

        /// <summary>
        /// Displays the current user's profile management page.
        /// </summary>
        /// <returns>Manage profile view</returns>
        [Route("Manage")]
        public async Task<IActionResult> Manage()
        {
            // Get current logged-in user ID
            var userId = _userManager.GetUserId(User);

            if (userId == null) return Unauthorized();

            // Retrieve current user's profile
            var user = await _userRepo.ReadProfileAsync(userId);

            return View(user);
        }
    }
}