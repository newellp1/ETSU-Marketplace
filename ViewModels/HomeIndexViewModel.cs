namespace ETSU_Marketplace.ViewModels
{
    public class HomeIndexViewModel
    {
        public List<ListingCardViewModel> LatestItemListings { get; set; } = new();
        public List<ListingCardViewModel> LatestLeaseListings { get; set; } = new();
    }
}