using ETSU_Marketplace.Models;
using ETSU_Marketplace.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETSU_Marketplace.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ItemAPIController : BaseAPIController<ItemListing, IItemListingRepository>
{
    private readonly ApplicationDbContext _db;

    public ItemAPIController(
        IItemListingRepository itemRepo,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db)
        : base(itemRepo, userManager)
    {
        _db = db;
    }

    protected override string GetRedirectPath() => "/Manage";

    // GET: api/ItemAPI/search?query=book
    [HttpGet("search")]
    public async Task<IActionResult> Search(string? query)
    {
        var listings = _db.ItemListings
            .Include(i => i.Images)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            listings = listings.Where(i =>
                i.Title.Contains(query) ||
                i.Description.Contains(query));
        }

        var results = await listings
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new
            {
                id = i.Id,
                title = i.Title,
                price = i.Price,
                description = i.Description,
                imageUrl = i.Images.Any()
                    ? i.Images.First().Path
                    : "/images/placeholder.png",
                detailsUrl = $"/Listings/Items/Details/{i.Id}?type=Item"
            })
            .ToListAsync();

        return Ok(results);
    }

    // POST: api/ItemAPI
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromForm] ItemListing entity,
        [FromForm] List<Category> selectedCategories,
        List<IFormFile> images)
    {
        entity.ListingCategories.Clear();

        foreach (var category in selectedCategories)
        {
            entity.ListingCategories.Add(new ListingCategory
            {
                Category = category
            });
        }

        return await CreateEntity(entity, images);
    }

    // PUT: api/ItemAPI/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        [FromForm] ItemListing entity,
        [FromForm] List<Category> selectedCategories,
        List<IFormFile> images)
    {
        if (id != entity.Id)
        {
            return BadRequest("Route ID does not match entity ID.");
        }

        entity.ListingCategories.Clear();

        foreach (var category in selectedCategories)
        {
            entity.ListingCategories.Add(new ListingCategory
            {
                ListingId = entity.Id,
                Category = category
            });
        }

        return await UpdateEntity(entity, images);
    }
}