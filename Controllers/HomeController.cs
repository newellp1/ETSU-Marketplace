using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ETSU_Marketplace.ViewModels;

namespace ETSU_Marketplace.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            const int take = 8;

            var latestItems = _db.ItemListings
                .Include(x => x.Images)
                .Include(x => x.ListingCategories)
                .AsNoTracking()
                .OrderBy(x => x.IsSold)
                .ThenByDescending(x => x.CreatedAt)
                .Take(take)
                .Select(x => new ListingCardViewModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    ShortDescription = x.Description,
                    Price = x.Price,
                    CreatedAt = x.CreatedAt,
                    IsSold = x.IsSold,
                    ListingType = "Item",
                    CategoryLabel = string.Join(", ",
                        x.ListingCategories.Select(lc => lc.Category.ToString())),
                    ConditionLabel = x.Condition.ToString(),
                    DetailsUrl = $"/Listings/Items/Details/{x.Id}?type=Item",
                    ImageUrls = x.Images.Select(i => i.Path).ToList(),
                })
                .ToList();

            var latestLeases = _db.LeaseListings
                .Include(x => x.Images)
                .AsNoTracking()
                .OrderBy(x => x.IsSold)
                .ThenByDescending(x => x.CreatedAt)
                .Take(take)
                .Select(x => new ListingCardViewModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    ShortDescription = x.Description,
                    Price = x.Price,
                    CreatedAt = x.CreatedAt,
                    IsSold = x.IsSold,
                    ListingType = "Lease",
                    CategoryLabel = "Lease",
                    ConditionLabel = null,
                    ImageUrls = x.Images.Select(img => img.Path).ToList(),
                    DetailsUrl = $"/Listings/Leases/Details/{x.Id}?type=Lease"
                })
                .ToList();

            var vm = new HomeIndexViewModel
            {
                LatestItemListings = latestItems,
                LatestLeaseListings = latestLeases
            };

            return View(vm);
        }

        public IActionResult Search(string? q, string? category, string? condition, decimal? minPrice, decimal? maxPrice, string? sort)
        {
            if (string.IsNullOrWhiteSpace(q))
                return RedirectToAction("Index");

            const int take = 200;

            var items = _db.ItemListings
                .Include(x => x.Images)
                .Include(x => x.ListingCategories)
                .AsNoTracking()
                .OrderBy(x => x.IsSold)
                .ThenByDescending(x => x.CreatedAt)
                .Take(take)
                .Select(x => new ListingCardViewModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    ShortDescription = x.Description,
                    Price = x.Price,
                    CreatedAt = x.CreatedAt,
                    IsSold = x.IsSold,
                    ListingType = "Item",
                    CategoryLabel = string.Join(", ",
                        x.ListingCategories.Select(lc => lc.Category.ToString())),
                    ConditionLabel = x.Condition.ToString(),
                    DetailsUrl = $"/Listings/Items/Details/{x.Id}?type=Item",
                    ImageUrls = x.Images.Select(i => i.Path).ToList()
                })
                .ToList();

            var leases = _db.LeaseListings
                .Include(x => x.Images)
                .AsNoTracking()
                .OrderBy(x => x.IsSold)
                .ThenByDescending(x => x.CreatedAt)
                .Take(take)
                .Select(x => new ListingCardViewModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    ShortDescription = x.Description,
                    Price = x.Price,
                    CreatedAt = x.CreatedAt,
                    IsSold = x.IsSold,
                    ListingType = "Lease",
                    ImageUrls = x.Images.Select(i => i.Path).ToList(),
                    DetailsUrl = $"/Listings/Leases/Details/{x.Id}?type=Lease"
                })
                .ToList();

            category = string.IsNullOrWhiteSpace(category) ? null : category.Trim();
            condition = string.IsNullOrWhiteSpace(condition) ? null : condition.Trim();
            sort = string.IsNullOrWhiteSpace(sort) ? null : sort.Trim();

            if (category != null)
            {
                items = items.Where(x => string.Equals(x.CategoryLabel, category, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (condition != null)
            {
                items = items.Where(x => string.Equals(x.ConditionLabel, condition, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (minPrice.HasValue)
            {
                items = items.Where(x => x.Price >= minPrice.Value).ToList();
                leases = leases.Where(x => x.Price >= minPrice.Value).ToList();
            }

            if (maxPrice.HasValue)
            {
                items = items.Where(x => x.Price <= maxPrice.Value).ToList();
                leases = leases.Where(x => x.Price <= maxPrice.Value).ToList();
            }

            items = sort switch
            {
                "price_asc" => items.OrderBy(x => x.IsSold).ThenBy(x => x.Price).ToList(),
                "price_desc" => items.OrderBy(x => x.IsSold).ThenByDescending(x => x.Price).ToList(),
                _ => items.OrderBy(x => x.IsSold).ThenByDescending(x => x.CreatedAt).ToList()
            };

            leases = sort switch
            {
                "price_asc" => leases.OrderBy(x => x.IsSold).ThenBy(x => x.Price).ToList(),
                "price_desc" => leases.OrderBy(x => x.IsSold).ThenByDescending(x => x.Price).ToList(),
                _ => leases.OrderBy(x => x.IsSold).ThenByDescending(x => x.CreatedAt).ToList()
            };

            ViewBag.SearchQuery = q;
            ViewBag.SelectedCategory = category;
            ViewBag.SelectedCondition = condition;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Sort = sort;

            return View("SearchResults", new HomeIndexViewModel
            {
                LatestItemListings = items,
                LatestLeaseListings = leases
            });
        }
    }
}