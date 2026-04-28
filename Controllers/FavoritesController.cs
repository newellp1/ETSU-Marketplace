using ETSU_Marketplace.Models;
using ETSU_Marketplace.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETSU_Marketplace.Controllers
{
    /// <summary>
    /// Controller responsible for managing user favorite listings.
    /// Allows users to view, add, and remove favorite items.
    /// </summary>
    [Authorize]
    [Route("Favorites")]
    public class FavoritesController : Controller
    {
        // Database context used to access listings and favorites
        private readonly ApplicationDbContext _db;

        // User manager used to get the current logged-in user
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Constructor that injects database context and user manager.
        /// </summary>
        /// <param name="db">Application database context</param>
        /// <param name="userManager">ASP.NET Identity user manager</param>
        public FavoritesController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        /// <summary>
        /// Displays the current user's favorite listings.
        /// Maps favorite listings into view models for display.
        /// </summary>
        /// <returns>Favorites view with listing cards</returns>
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            // Get current logged-in user ID
            var userId = _userManager.GetUserId(User);

            if (userId == null) return Unauthorized();

            // Retrieve favorite listings with related data (images, user, avatar)
            var favorites = await _db.FavoriteListings
                .Include(f => f.Listing)
                    .ThenInclude(l => l!.Images)
                .Include(f => f.Listing)
                    .ThenInclude(l => l!.User)
                        .ThenInclude(u => u!.Avatar)
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            var vm = new HomeIndexViewModel();

            // Convert each favorite listing into a card view model
            foreach (var favorite in favorites)
            {
                if (favorite.Listing == null) continue;

                var listing = favorite.Listing;

                var card = new ListingCardViewModel
                {
                    Id = listing.Id,
                    Title = listing.Title,
                    ShortDescription = listing.Description,
                    Price = listing.Price,
                    CreatedAt = listing.CreatedAt,
                    IsSold = listing.IsSold,

                    // Mark as favorited for UI display
                    IsFavorited = true,

                    // Map image paths
                    ImageUrls = listing.Images.Select(i => i.Path).ToList(),

                    // Display seller name
                    Poster = listing.User != null
                        ? $"{listing.User.FirstName} {listing.User.LastName}"
                        : "",

                    // Use avatar or fallback image
                    PosterAvatar = listing.User?.Avatar?.Path ?? "/images/placeholder.png"
                };

                // Handle item listings
                if (listing is ItemListing item)
                {
                    // Load categories for item
                    await _db.Entry(item).Collection(i => i.ListingCategories).LoadAsync();

                    card.ListingType = "Item";

                    // Combine category names
                    card.CategoryLabel = string.Join(", ",
                        item.ListingCategories.Select(lc => lc.Category.ToString()));

                    // Set condition label
                    card.ConditionLabel = item.Condition.ToString();

                    // Build details page URL
                    card.DetailsUrl = $"/Listings/Items/Details/{item.Id}?type=Item";

                    vm.LatestItemListings.Add(card);
                }
                // Handle lease listings
                else if (listing is LeaseListing lease)
                {
                    card.ListingType = "Lease";
                    card.CategoryLabel = "Lease";

                    // Build details page URL
                    card.DetailsUrl = $"/Listings/Leases/Details/{lease.Id}?type=Lease";

                    vm.LatestLeaseListings.Add(card);
                }
            }

            return View(vm);
        }

        [HttpPost("toggle/{listingId}")]
        public async Task<IActionResult> Toggle(int listingId, string? returnUrl)
        {
            // Get current logged-in user ID
            var userId = _userManager.GetUserId(User);

            if (userId == null) return Unauthorized();

            // Ensure the listing exists
            var listingExists = await _db.Listings.AnyAsync(l => l.Id == listingId);
            if (!listingExists) return NotFound();

            // Check if listing is already favorited
            var existingFavorite = await _db.FavoriteListings
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ListingId == listingId);

            bool isFavorited;

            if (existingFavorite == null)
            {
                // Add new favorite
                _db.FavoriteListings.Add(new FavoriteListing
                {
                    UserId = userId,
                    ListingId = listingId
                });

                isFavorited = true;
            }
            else
            {
                // Remove existing favorite
                _db.FavoriteListings.Remove(existingFavorite);

                isFavorited = false;
            }

            await _db.SaveChangesAsync();

            // If request came from AJAX, return JSON response
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    isFavorited = isFavorited
                });
            }

            // Redirect back if return URL is valid
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            // Default redirect to favorites page
            return RedirectToAction("Index");
        }
    }
}