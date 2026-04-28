using ETSU_Marketplace.Models;
using ETSU_Marketplace.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETSU_Marketplace.Controllers;

/// <summary>
/// API controller for handling item listing operations.
/// Supports CRUD operations and AJAX-based search functionality.
/// Inherits shared logic from BaseAPIController.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ItemAPIController : BaseAPIController<ItemListing, IItemListingRepository>
{
    // Database context for querying listings (used for search)
    private readonly ApplicationDbContext _db;

    /// <summary>
    /// Constructor that injects repository, user manager, and database context.
    /// </summary>
    /// <param name="itemRepo">Repository for item listings</param>
    /// <param name="userManager">ASP.NET Identity user manager</param>
    /// <param name="db">Application database context</param>
    public ItemAPIController(
        IItemListingRepository itemRepo,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db)
        : base(itemRepo, userManager)
    {
        _db = db;
    }

    /// <summary>
    /// Specifies where to redirect after create/update/delete actions.
    /// </summary>
    protected override string GetRedirectPath() => "/Manage";

    /// <summary>
    /// Searches item listings based on a query string.
    /// Used for AJAX live search without reloading the page.
    /// </summary>
    /// <param name="query">Search term entered by the user</param>
    /// <returns>JSON list of matching item listings</returns>
    // GET: api/ItemAPI/search?query=book
    [HttpGet("search")]
    public async Task<IActionResult> Search(string? query)
    {
        // Start query and include images
        var listings = _db.ItemListings
            .Include(i => i.Images)
            .AsQueryable();

        // Apply search filter if query exists
        if (!string.IsNullOrWhiteSpace(query))
        {
            listings = listings.Where(i =>
                i.Title.Contains(query) ||
                i.Description.Contains(query));
        }

        // Project results into lightweight anonymous objects for JSON response
        var results = await listings
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new
            {
                id = i.Id,
                title = i.Title,
                price = i.Price,
                description = i.Description,

                // Use first image or fallback placeholder
                imageUrl = i.Images.Any()
                    ? i.Images.First().Path
                    : "/images/placeholder.png",

                // Link to item details page
                detailsUrl = $"/Listings/Items/Details/{i.Id}?type=Item"
            })
            .ToListAsync();

        return Ok(results);
    }

    /// <summary>
    /// Creates a new item listing.
    /// Accepts form data including categories and images.
    /// </summary>
    /// <param name="entity">Item listing data</param>
    /// <param name="selectedCategories">Selected categories</param>
    /// <param name="images">Uploaded images</param>
    /// <returns>Redirect after creation</returns>
    // POST: api/ItemAPI
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromForm] ItemListing entity,
        [FromForm] List<Category> selectedCategories,
        List<IFormFile> images)
    {
        // Clear existing categories to avoid duplicates
        entity.ListingCategories.Clear();

        // Add selected categories to listing
        foreach (var category in selectedCategories)
        {
            entity.ListingCategories.Add(new ListingCategory
            {
                Category = category
            });
        }

        // Call base method to handle creation logic
        return await CreateEntity(entity, images);
    }

    /// <summary>
    /// Updates an existing item listing.
    /// Validates ID and updates categories and images.
    /// </summary>
    /// <param name="id">Listing ID from route</param>
    /// <param name="entity">Updated listing data</param>
    /// <param name="selectedCategories">Updated categories</param>
    /// <param name="images">Updated images</param>
    /// <returns>Redirect after update</returns>
    // PUT: api/ItemAPI/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        [FromForm] ItemListing entity,
        [FromForm] List<Category> selectedCategories,
        List<IFormFile> images)
    {
        // Ensure route ID matches entity ID
        if (id != entity.Id)
        {
            return BadRequest("Route ID does not match entity ID.");
        }

        // Clear existing categories
        entity.ListingCategories.Clear();

        // Re-add updated categories
        foreach (var category in selectedCategories)
        {
            entity.ListingCategories.Add(new ListingCategory
            {
                ListingId = entity.Id,
                Category = category
            });
        }

        // Call base method to handle update logic
        return await UpdateEntity(entity, images);
    }
}