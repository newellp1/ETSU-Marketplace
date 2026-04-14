using ETSU_Marketplace.Models;

namespace ETSU_Marketplace.Services;

public interface ILeaseListingRepository : IListingRepository<LeaseListing>
{
    // This interface "inherits" other methods
    new Task UpdateAsync(int id, LeaseListing updatedILease, List<IFormFile> images);
}
