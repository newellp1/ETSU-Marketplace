using Microsoft.AspNetCore.Identity;

namespace ETSU_Marketplace.Models;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string? Bio { get; set; }

    public int? AvatarId { get; set; }
    public Image Avatar { get; set; } = new Image { Path = "/images/default-avatar.jpg" };

    public ICollection<Listing> Listings { get; set; } = new List<Listing>();
    public ICollection<FavoriteListing> FavoriteListings { get; set; } = new List<FavoriteListing>();

    public ICollection<Conversation> SellerConversations { get; set; } = new List<Conversation>();
    public ICollection<Conversation> BuyerConversations { get; set; } = new List<Conversation>();
    public ICollection<ChatMessage> SentMessages { get; set; } = new List<ChatMessage>();
}