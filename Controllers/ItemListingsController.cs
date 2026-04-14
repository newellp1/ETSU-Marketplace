using Microsoft.AspNetCore.Mvc;
using ETSU_Marketplace.ViewModels;
using Microsoft.AspNetCore.Authorization;
using ETSU_Marketplace.Services;
using Microsoft.AspNetCore.Identity;
using ETSU_Marketplace.Models;
using Microsoft.EntityFrameworkCore;

namespace ETSU_Marketplace.Controllers
{
    [Authorize]
    [Route("Listings/Items/")]
    public class ItemListingsController : BaseListingsController<ItemListing, IItemListingRepository>
    {
        private readonly ApplicationDbContext _db;

        public ItemListingsController(
            IItemListingRepository itemRepo,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext db)
            : base(itemRepo, userManager)
        {
            _db = db;
        }

        [HttpGet("")]
        public async Task<IActionResult> Items(
            string? category,
            string? condition,
            decimal? minPrice,
            decimal? maxPrice,
            string? sort,
            string? q)
        {
            var items = await _repository.ReadAllAsync();
            var vms = new List<ListingCardViewModel>();
            var homeIndexVM = new HomeIndexViewModel();

            var currentUserId = CurrentUserId;
            var favoriteIds = currentUserId == null
                ? new HashSet<int>()
                : (await _db.FavoriteListings
                    .Where(f => f.UserId == currentUserId)
                    .Select(f => f.ListingId)
                    .ToListAsync()).ToHashSet();

            foreach (var item in items)
            {
                var vm = MapToCardViewModel(item, false);
                vm.ListingType = "Item";
                vm.CategoryLabel = string.Join(", ",
                    item.ListingCategories.Select(lc => lc.Category.ToString()));
                vm.ConditionLabel = item.Condition.ToString();
                vm.DetailsUrl = $"/Listings/Items/Details/{item.Id}?type=Item";
                vm.IsFavorited = favoriteIds.Contains(item.Id);
                vms.Add(vm);
            }

            category = string.IsNullOrWhiteSpace(category) ? null : category.Trim();
            condition = string.IsNullOrWhiteSpace(condition) ? null : condition.Trim();
            q = string.IsNullOrWhiteSpace(q) ? null : q.Trim();
            sort = string.IsNullOrWhiteSpace(sort) ? null : sort.Trim();

            if (category != null)
            {
                vms = vms
                    .Where(l =>
                        !string.IsNullOrWhiteSpace(l.CategoryLabel) &&
                        l.CategoryLabel.Split(", ")
                            .Any(c => string.Equals(c, category, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            if (condition != null)
            {
                vms = vms
                    .Where(l => string.Equals(l.ConditionLabel, condition, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (minPrice.HasValue)
            {
                vms = vms.Where(l => l.Price >= minPrice.Value).ToList();
            }

            if (maxPrice.HasValue)
            {
                vms = vms.Where(l => l.Price <= maxPrice.Value).ToList();
            }

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

            vms = sort switch
            {
                "price_asc" => vms.OrderBy(l => l.IsSold).ThenBy(l => l.Price).ToList(),
                "price_desc" => vms.OrderBy(l => l.IsSold).ThenByDescending(l => l.Price).ToList(),
                _ => vms.OrderBy(l => l.IsSold).ThenByDescending(l => l.CreatedAt).ToList()
            };

            ViewBag.SelectedCategory = category;
            ViewBag.SelectedCondition = condition;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Sort = sort;
            ViewBag.SearchQuery = q;

            foreach (var v in vms)
            {
                homeIndexVM.LatestItemListings.Add(v);
            }

            return View(homeIndexVM);
        }

        [Route("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var item = await _repository.ReadAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            var vm = MapToCardViewModel(item, item.UserId == CurrentUserId);
            vm.ListingType = "Item";
            vm.CategoryLabel = string.Join(", ",
                item.ListingCategories.Select(lc => lc.Category.ToString()));
            vm.ConditionLabel = item.Condition.ToString();
            vm.Poster = $"{item.User!.FirstName} {item.User.LastName}";
            vm.PosterAvatar = item.User?.Avatar?.Path ?? "/images/placeholder.png";

            if (CurrentUserId != null)
            {
                vm.IsFavorited = await _db.FavoriteListings
                    .AnyAsync(f => f.UserId == CurrentUserId && f.ListingId == item.Id);
            }

            return View(vm);
        }

        [Route("Create")]
        public IActionResult Create()
        {
            return View();
        }

        [Route("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            if (CurrentUserId == null) return Unauthorized();
            var item = await _repository.ReadAsync(id);

            if (item == null) return NotFound();

            if (!IsOwner(item))
            {
                return RedirectToAction("Index", "Home");
            }

            return View(item);
        }

        [Route("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (CurrentUserId == null) return Unauthorized();
            var item = await _repository.ReadAsync(id);

            if (item == null) return NotFound();

            if (!IsOwner(item))
            {
                return RedirectToAction("Index", "Home");
            }

            return View(item);
        }

    }
}