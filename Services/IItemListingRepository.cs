using ETSU_Marketplace.Models;

namespace ETSU_Marketplace.Services;

public interface IItemListingRepository : IListingRepository<ItemListing>
{
    // This interface "inherits" other methods
    new Task UpdateAsync(int id, ItemListing updatedItem, List<IFormFile> images);
}
