using ETSU_Marketplace.Models;
using ETSU_Marketplace.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ETSU_Marketplace.Controllers;

[Route("api/[controller]")]
public class ItemAPIController : BaseAPIController<ItemListing, IItemListingRepository>
{
    public ItemAPIController(IItemListingRepository itemRepo, UserManager<ApplicationUser> userManager)
        : base(itemRepo, userManager) { }

    protected override string GetRedirectPath() => "/Manage";

    [HttpPost("create")]
    public async Task<IActionResult> Post(
        [FromForm] ItemListing entity,
        [FromForm] List<Category> SelectedCategories,
        List<IFormFile> images)
    {
        entity.ListingCategories.Clear();

        foreach (var category in SelectedCategories)
        {
            entity.ListingCategories.Add(new ListingCategory
            {
                Category = category
            });
        }

        return await CreateEntity(entity, images);
    }

    [HttpPost("update")]
    public async Task<IActionResult> Put(
        [FromForm] ItemListing entity,
        [FromForm] List<Category> SelectedCategories,
        List<IFormFile> images)
    {
        entity.ListingCategories.Clear();

        foreach (var category in SelectedCategories)
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