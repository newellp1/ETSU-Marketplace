using ETSU_Marketplace.Models;
using Microsoft.EntityFrameworkCore;

namespace ETSU_Marketplace.Services;

public class DbItemListingRepository : DbListingRepository<ItemListing>, IItemListingRepository
{
    public DbItemListingRepository(ApplicationDbContext db, IFileStorageService fss)
        : base(db, fss)
    {
    }

    public override async Task<ItemListing> CreateAsync(ItemListing newItem, List<IFormFile> images, string userId)
    {
        newItem.UserId = userId;

        if (images != null && images.Any())
        {
            foreach (var file in images)
            {
                string path = await _fss.ProcessImageUpload(file);
                newItem.Images.Add(new Image { Path = path });
            }
        }

        await _db.ItemListings.AddAsync(newItem);
        await _db.SaveChangesAsync();

        return newItem;
    }

    public override async Task<ICollection<ItemListing>> ReadAllAsync()
    {
        return await _db.ItemListings
        .Include(i => i.Images)
        .Include(i => i.ListingCategories)
        .Include(i => i.User)
            .ThenInclude(u => u!.Avatar)
        .ToListAsync();
    }

    public override async Task<ItemListing?> ReadAsync(int id)
    {
        return await _db.ItemListings
        .Include(i => i.Images)
        .Include(i => i.ListingCategories)
        .Include(i => i.User)
            .ThenInclude(u => u!.Avatar)
        .FirstOrDefaultAsync(i => i.Id == id);
    }

    public override async Task UpdateAsync(int id, ItemListing updatedItem, List<IFormFile> images)
    {
        await base.UpdateAsync(id, updatedItem, images);

        var existing = await _db.ItemListings
            .Include(i => i.ListingCategories)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (existing != null)
        {
            existing.Condition = updatedItem.Condition;

            existing.ListingCategories.Clear();

            foreach (var listingCategory in updatedItem.ListingCategories)
            {
                existing.ListingCategories.Add(new ListingCategory
                {
                    ListingId = existing.Id,
                    Category = listingCategory.Category
                });
            }

            await _db.SaveChangesAsync();
        }
    }
}