using ETSU_Marketplace.Models;

public class ItemListing : Listing
{
    public Condition Condition {get; set;}

    //One to Many relationship with Category
    public ICollection<ListingCategory> ListingCategories { get; set; } = new List<ListingCategory>();
}