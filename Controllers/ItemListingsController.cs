using Microsoft.AspNetCore.Mvc;
using ETSU_Marketplace.ViewModels;
using Microsoft.AspNetCore.Authorization;
using ETSU_Marketplace.Services;
using Microsoft.AspNetCore.Identity;
using ETSU_Marketplace.Models;
using Microsoft.EntityFrameworkCore;

namespace ETSU_Marketplace.Controllers
{
    /// <summary>
    /// Controller responsible for displaying and managing item listings.
    /// Handles filtering, searching, viewing details, and ownership-based actions.
    /// </summary>
    [Authorize]
    [Route("Listings/Items/")]
    public class ItemListingsController : BaseListingsController<ItemListing, IItemListingRepository>
    {
        // Database context for favorites and additional queries
        private readonly ApplicationDbContext _db;

        /// <summary>
        /// Constructor that injects repository, user manager, and database context.
        /// </summary>
        public ItemListingsController(
            IItemListingRepository itemRepo,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext db)
            : base(itemRepo, userManager)
        {
            _db = db;
        }

        /// <summary>
        /// Displays all item listings with filtering, sorting, and search options.
        /// </summary>
        [HttpGet("")]
        public async Task<IActionResult> Items(
            string? category,
            string? condition,
            decimal? minPrice,
            decimal? maxPrice,
            string? sort,
            string? q)
        {
            // Retrieve all items from repository
            var items = await _repository.ReadAllAsync();

            var vms = new List<ListingCardViewModel>();
            var homeIndexVM = new HomeIndexViewModel();

            var currentUserId = CurrentUserId;

            // Get list of favorited listing IDs for current user
            var favoriteIds = currentUserId == null
                ? new HashSet<int>()
                : (await _db.FavoriteListings
                    .Where(f => f.UserId == currentUserId)
                    .Select(f => f.ListingId)
                    .ToListAsync()).ToHashSet();

            // Convert each item into a view model
            foreach (var item in items)
            {
                var vm = MapToCardViewModel(item, false);

                vm.ListingType = "Item";

                // Combine categories into a readable string
                vm.CategoryLabel = string.Join(", ",
                    item.ListingCategories.Select(lc => lc.Category.ToString()));

                vm.ConditionLabel = item.Condition.ToString();

                // Link to item details page
                vm.DetailsUrl = $"/Listings/Items/Details/{item.Id}?type=Item";

                // Mark if user has favorited this item
                vm.IsFavorited = favoriteIds.Contains(item.Id);

                vms.Add(vm);
            }

            // Clean filter inputs
            category = string.IsNullOrWhiteSpace(category) ? null : category.Trim();
            condition = string.IsNullOrWhiteSpace(condition) ? null : condition.Trim();
            q = string.IsNullOrWhiteSpace(q) ? null : q.Trim();
            sort = string.IsNullOrWhiteSpace(sort) ? null : sort.Trim();

            // Apply category filter
            if (category != null)
            {
                vms = vms
                    .Where(l =>
                        !string.IsNullOrWhiteSpace(l.CategoryLabel) &&
                        l.CategoryLabel.Split(", ")
                            .Any(c => string.Equals(c, category, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            // Apply condition filter
            if (condition != null)
            {
                vms = vms
                    .Where(l => string.Equals(l.ConditionLabel, condition, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Apply minimum price filter
            if (minPrice.HasValue)
            {
                vms = vms.Where(l => l.Price >= minPrice.Value).ToList();
            }

            // Apply maximum price filter
            if (maxPrice.HasValue)
            {
                vms = vms.Where(l => l.Price <= maxPrice.Value).ToList();
            }

            // Apply keyword search filter
            if (q != null)
            {
                vms = vms
                    .Where(l =>
                        (!string.IsNullOrWhiteSpace(l.Title) &&
                         l.Title.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrWhiteSpace(l.ShortDescription) &&
                         l.ShortDescription.Contains(q, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            // Apply sorting
            vms = sort switch
            {
                "price_asc" => vms.OrderBy(l => l.IsSold).ThenBy(l => l.Price).ToList(),
                "price_desc" => vms.OrderBy(l => l.IsSold).ThenByDescending(l => l.Price).ToList(),
                _ => vms.OrderBy(l => l.IsSold).ThenByDescending(l => l.CreatedAt).ToList()
            };

            // Store filter values for UI display
            ViewBag.SelectedCategory = category;
            ViewBag.SelectedCondition = condition;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Sort = sort;
            ViewBag.SearchQuery = q;

            // Add filtered results to view model
            foreach (var v in vms)
            {
                homeIndexVM.LatestItemListings.Add(v);
            }

            return View(homeIndexVM);
        }

        /// <summary>
        /// Displays details for a specific item listing.
        /// </summary>
        [Route("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var item = await _repository.ReadAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            // Map item to view model
            var vm = MapToCardViewModel(item, item.UserId == CurrentUserId);

            vm.ListingType = "Item";
            vm.CategoryLabel = string.Join(", ",
                item.ListingCategories.Select(lc => lc.Category.ToString()));
            vm.ConditionLabel = item.Condition.ToString();

            // Display seller info
            vm.Poster = $"{item.User!.FirstName} {item.User.LastName}";
            vm.PosterAvatar = item.User?.Avatar?.Path ?? "/images/placeholder.png";

            // Check if current user has favorited this item
            if (CurrentUserId != null)
            {
                vm.IsFavorited = await _db.FavoriteListings
                    .AnyAsync(f => f.UserId == CurrentUserId && f.ListingId == item.Id);
            }

            return View(vm);
        }

        /// <summary>
        /// Displays create listing page.
        /// </summary>
        [Route("Create")]
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Displays edit page for a listing (only if user owns it).
        /// </summary>
        [Route("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            if (CurrentUserId == null) return Unauthorized();

            var item = await _repository.ReadAsync(id);

            if (item == null) return NotFound();

            // Ensure only owner can edit
            if (!IsOwner(item))
            {
                return RedirectToAction("Index", "Home");
            }

            return View(item);
        }

        /// <summary>
        /// Displays delete confirmation page (only if user owns it).
        /// </summary>
        [Route("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (CurrentUserId == null) return Unauthorized();

            var item = await _repository.ReadAsync(id);

            if (item == null) return NotFound();

            // Ensure only owner can delete
            if (!IsOwner(item))
            {
                return RedirectToAction("Index", "Home");
            }

            return View(item);
        }
    }
}