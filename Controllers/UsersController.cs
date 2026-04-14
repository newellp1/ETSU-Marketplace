using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ETSU_Marketplace.ViewModels;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Identity;
using ETSU_Marketplace.Models;
using ETSU_Marketplace.Services;

namespace ETSU_Marketplace.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserRepository _userRepo;

        public UsersController(ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            IUserRepository userRepo)
        {
            _db = db;
            _userManager = userManager;
            _userRepo = userRepo;
        }

        public async Task<IActionResult> Details(string id)
        {
            var user = await _userRepo.ReadProfileAsync(id);
            if (user == null)
                return NotFound();
            return View(user);
        }

        [Route("Manage")]
        public async Task<IActionResult> Manage()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var user = await _userRepo.ReadProfileAsync(userId);

            return View(user);
        }

    }
}