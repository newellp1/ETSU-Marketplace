using Microsoft.AspNetCore.Mvc;
using ETSU_Marketplace.ViewModels;
using Microsoft.AspNetCore.Authorization;
using ETSU_Marketplace.Services;
using Microsoft.AspNetCore.Identity;
using ETSU_Marketplace.Models;

namespace ETSU_Marketplace.Controllers
{
    /// <summary>
    /// Controller responsible for displaying and managing lease listings.
    /// Handles viewing, filtering, and ownership-based actions for leases.
    /// </summary>
    [Authorize]
    [Route("Listings/Leases")]
    public class LeaseListingsController : BaseListingsController<LeaseListing, ILeaseListingRepository>
    {
        /// <summary>
        /// Constructor that injects repository and user manager.
        /// </summary>
        public LeaseListingsController(
            ILeaseListingRepository leaseRepo,
            UserManager<ApplicationUser> userManager)
            : base(leaseRepo, userManager) { }

        /// <summary>
        /// Displays details for a specific lease listing.
        /// </summary>
        /// <param name="id">Lease listing ID</param>
        /// <returns>Details view</returns>
        [Route("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var lease = await _repository.ReadAsync(id);

            if (lease == null)
            {
                return NotFound();
            }

            // Map lease to view model
            var vm = MapToCardViewModel(lease, lease.UserId == CurrentUserId);

            vm.ListingType = "Lease";

            // Display poster information
            vm.Poster = $"{lease.User!.FirstName} {lease.User.LastName}";
            vm.PosterAvatar = lease.User?.Avatar?.Path ?? "/images/placeholder.png";

            return View(vm);
        }

        /// <summary>
        /// Displays create lease listing page.
        /// </summary>
        [Route("Create")]
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Displays edit page for a lease listing (only if user owns it).
        /// </summary>
        /// <param name="id">Lease listing ID</param>
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
        /// <param name="id">Lease listing ID</param>
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

        /// <summary>
        /// Displays all lease listings with filtering, search, and sorting options.
        /// </summary>
        /// <param name="minPrice">Minimum price filter</param>
        /// <param name="maxPrice">Maximum price filter</param>
        /// <param name="sort">Sorting option</param>
        /// <param name="q">Search query</param>
        /// <returns>Lease listings view</returns>
        [HttpGet("")]
        public async Task<IActionResult> Leases(
            decimal? minPrice,
            decimal? maxPrice,
            string? sort,
            string? q)
        {
            // Retrieve all lease listings
            var leases = await _repository.ReadAllAsync();

            var vms = new List<ListingCardViewModel>();
            var homeIndexVM = new HomeIndexViewModel();

            // Convert each lease into a view model
            foreach (var lease in leases)
            {
                var vm = MapToCardViewModel(lease, false);

                vm.ListingType = "Lease";

                // Build details page URL
                vm.DetailsUrl = $"/Listings/Leases/Details/{lease.Id}?type=Lease";

                vms.Add(vm);
            }

            // Clean inputs
            q = string.IsNullOrWhiteSpace(q) ? null : q.Trim();
            sort = string.IsNullOrWhiteSpace(sort) ? null : sort.Trim();

            // Apply price filters
            if (minPrice.HasValue)
            {
                vms = vms.Where(l => l.Price >= minPrice.Value).ToList();
            }

            if (maxPrice.HasValue)
            {
                vms = vms.Where(l => l.Price <= maxPrice.Value).ToList();
            }

            // Apply search filter
            if (q != null)
            {
                vms = vms.Where(l =>
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

            // Store filters for UI display
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Sort = sort;
            ViewBag.SearchQuery = q;

            // Add results to view model
            foreach (var v in vms)
            {
                homeIndexVM.LatestLeaseListings.Add(v);
            }

            return View(homeIndexVM);
        }
    }
}