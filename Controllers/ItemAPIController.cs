using ETSU_Marketplace.Models;
using ETSU_Marketplace.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ETSU_Marketplace.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ItemAPIController : BaseAPIController<ItemListing, IItemListingRepository>
{
    public ItemAPIController(
        IItemListingRepository itemRepo,
        UserManager<ApplicationUser> userManager)
        : base(itemRepo, userManager) { }

    protected override string GetRedirectPath() => "/Manage";

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