using System.ComponentModel.DataAnnotations.Schema;

namespace ETSU_Marketplace.Models
{
    public class ListingCategory
    {
        public int ListingId { get; set; }
        public ItemListing Listing { get; set; } = null!;

        public Category Category { get; set; }
    }
}