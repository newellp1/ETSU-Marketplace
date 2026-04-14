using ETSU_Marketplace.Models;
using ETSU_Marketplace.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETSU_Marketplace.Controllers
{
    [Authorize]
    [Route("Favorites")]
    public class FavoritesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public FavoritesController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

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
                    IsFavorited = true,
                    ImageUrls = listing.Images.Select(i => i.Path).ToList(),
                    Poster = listing.User != null ? $"{listing.User.FirstName} {listing.User.LastName}" : "",
                    PosterAvatar = listing.User?.Avatar?.Path ?? "/images/placeholder.png"
                };

                if (listing is ItemListing item)
                {
                    await _db.Entry(item).Collection(i => i.ListingCategories).LoadAsync();

                    card.ListingType = "Item";
                    card.CategoryLabel = string.Join(", ", item.ListingCategories.Select(lc => lc.Category.ToString()));
                    card.ConditionLabel = item.Condition.ToString();
                    card.DetailsUrl = $"/Listings/Items/Details/{item.Id}?type=Item";
                    vm.LatestItemListings.Add(card);
                }
                else if (listing is LeaseListing lease)
                {
                    card.ListingType = "Lease";
                    card.CategoryLabel = "Lease";
                    card.DetailsUrl = $"/Listings/Leases/Details/{lease.Id}?type=Lease";
                    vm.LatestLeaseListings.Add(card);
                }
            }

            return View(vm);
        }

        [HttpPost("toggle/{listingId}")]
        public async Task<IActionResult> Toggle(int listingId, string? returnUrl)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var listingExists = await _db.Listings.AnyAsync(l => l.Id == listingId);
            if (!listingExists) return NotFound();

            var existingFavorite = await _db.FavoriteListings
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ListingId == listingId);

            if (existingFavorite == null)
            {
                _db.FavoriteListings.Add(new FavoriteListing
                {
                    UserId = userId,
                    ListingId = listingId
                });
            }
            else
            {
                _db.FavoriteListings.Remove(existingFavorite);
            }

            await _db.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrWhiteSpace(referer))
            {
                return Redirect(referer);
            }

            return RedirectToAction("Index");
        }
    }
}