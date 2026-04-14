using Microsoft.AspNetCore.Mvc;
using ETSU_Marketplace.ViewModels;
using Microsoft.AspNetCore.Authorization;
using ETSU_Marketplace.Services;
using Microsoft.AspNetCore.Identity;
using ETSU_Marketplace.Models;

namespace ETSU_Marketplace.Controllers
{
    [Authorize]
    [Route("Listings/Leases")]
    public class LeaseListingsController : BaseListingsController<LeaseListing, ILeaseListingRepository>
    {
        public LeaseListingsController(ILeaseListingRepository leaseRepo, UserManager<ApplicationUser> userManager)
            : base(leaseRepo, userManager) { }

        [Route("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var lease = await _repository.ReadAsync(id);

            if (lease == null)
            {
                return NotFound();
            }

            var vm = MapToCardViewModel(lease, lease.UserId == CurrentUserId);
            vm.ListingType = "Lease";
            vm.Poster = $"{lease.User!.FirstName} {lease.User.LastName}";
            vm.PosterAvatar = lease.User?.Avatar?.Path ?? "/images/placeholder.png";

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

        [HttpGet("")]
        public async Task<IActionResult> Leases(decimal? minPrice, decimal? maxPrice, string? sort, string? q)
        {
            var leases = await _repository.ReadAllAsync();
            var vms = new List<ListingCardViewModel>();
            var homeIndexVM = new HomeIndexViewModel();

            foreach (var lease in leases)
            {
                var vm = MapToCardViewModel(lease, false);
                vm.ListingType = "Lease";
                vm.DetailsUrl = $"/Listings/Leases/Details/{lease.Id}?type=Lease";
                vms.Add(vm);
            }

            q = string.IsNullOrWhiteSpace(q) ? null : q.Trim();
            sort = string.IsNullOrWhiteSpace(sort) ? null : sort.Trim();

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
                vms = vms.Where(l =>
                    (!string.IsNullOrWhiteSpace(l.Title) && l.Title.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(l.ShortDescription) && l.ShortDescription.Contains(q, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            vms = sort switch
            {
                "price_asc" => vms.OrderBy(l => l.IsSold).ThenBy(l => l.Price).ToList(),
                "price_desc" => vms.OrderBy(l => l.IsSold).ThenByDescending(l => l.Price).ToList(),
                _ => vms.OrderBy(l => l.IsSold).ThenByDescending(l => l.CreatedAt).ToList()
            };

            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Sort = sort;
            ViewBag.SearchQuery = q;

            foreach (var v in vms)
            {
                homeIndexVM.LatestLeaseListings.Add(v);
            }

            return View(homeIndexVM);
        }
    }
}