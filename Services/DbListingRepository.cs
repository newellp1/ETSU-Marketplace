using Microsoft.EntityFrameworkCore;
using ETSU_Marketplace.Models;

namespace ETSU_Marketplace.Services;

public class DbListingRepository<T> : IListingRepository<T> where T : Listing
{
    protected readonly ApplicationDbContext _db;
    protected readonly IFileStorageService _fss;

    public DbListingRepository(ApplicationDbContext db, IFileStorageService fss)
    {
        _db = db;
        _fss = fss;
    }

    public virtual async Task<T> CreateAsync(T newListing, List<IFormFile> images, string userId)
    {
        newListing.UserId = userId;

        if (images != null && images.Any())
        {
            foreach (var file in images)
            {
                string path = await _fss.ProcessImageUpload(file);
                newListing.Images.Add(new Image { Path = path });
            }
        }

        await _db.Set<T>().AddAsync(newListing);
        await _db.SaveChangesAsync();
        return newListing;
    }

    public virtual async Task DeleteAsync(int id)
    {
        var listingToDelete = await ReadAsync(id);
        if (listingToDelete != null)
        {
            if (listingToDelete.Images != null)
            {
                foreach (var img in listingToDelete.Images)
                {
                    var filePath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        img.Path.TrimStart('/'));

                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
            }

            _db.Set<T>().Remove(listingToDelete);
            await _db.SaveChangesAsync();
        }
    }

    public virtual async Task<ICollection<T>> ReadAllAsync()
    {
        return await _db.Set<T>()
            .Include(l => l.Images)
            .Include(l => l.User)
                .ThenInclude(u => u!.Avatar)
            .OrderBy(l => l.IsSold)
            .ThenByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    public virtual async Task<T?> ReadAsync(int id)
    {
        return await _db.Set<T>()
            .Include(l => l.Images)
            .Include(l => l.User)
                .ThenInclude(u => u!.Avatar)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public virtual async Task<ICollection<T>> ReadUserPostsAsync(string userId)
    {
        return await _db.Set<T>()
            .Include(l => l.Images)
            .Include(l => l.User)
                .ThenInclude(u => u!.Avatar)
            .Where(l => l.UserId == userId)
            .OrderBy(l => l.IsSold)
            .ThenByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    public virtual async Task ToggleSoldStatusAsync(int id)
    {
        var existing = await _db.Set<T>().FirstOrDefaultAsync(l => l.Id == id);

        if (existing == null) return;

        existing.IsSold = !existing.IsSold;
        await _db.SaveChangesAsync();
    }

    public virtual async Task UpdateAsync(int id, T updatedListing, List<IFormFile> newImages)
    {
        var existing = await _db.Set<T>()
            .Include(l => l.Images)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (existing == null) return;

        existing.Title = updatedListing.Title;
        existing.Description = updatedListing.Description;
        existing.Price = updatedListing.Price;
        existing.IsSold = updatedListing.IsSold;

        if (newImages != null && newImages.Any())
        {
            foreach (var img in existing.Images)
            {
                _fss.DeleteImage(img.Path);
            }

            _db.Images.RemoveRange(existing.Images);
            existing.Images.Clear();

            foreach (var file in newImages)
            {
                var dbPath = await _fss.ProcessImageUpload(file);
                existing.Images.Add(new Image { Path = dbPath });
            }
        }

        await _db.SaveChangesAsync();
    }
}