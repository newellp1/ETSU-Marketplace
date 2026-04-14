using System.ComponentModel.DataAnnotations;
using ETSU_Marketplace.Models;

public class ItemListing : Listing
{
    [Required(ErrorMessage = "Please select an item condition.")]
    public Condition Condition { get; set; }

    // Many-to-many relationship with Category through ListingCategory
    public ICollection<ListingCategory> ListingCategories { get; set; } = new List<ListingCategory>();
}
