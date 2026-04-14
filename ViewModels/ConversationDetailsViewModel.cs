using ETSU_Marketplace.Models;

namespace ETSU_Marketplace.ViewModels
{
    public class ConversationDetailsViewModel
    {
        public int ConversationId { get; set; }
        public int ListingId { get; set; }
        public string ListingTitle { get; set; } = "";
        public string ListingType { get; set; } = "";
        public string OtherUserName { get; set; } = "";
        public string OtherUserAvatar { get; set; } = "/images/default-avatar.jpg";
        public string DetailsUrl { get; set; } = "";
        public bool IsSold { get; set; }
        public bool IsSellerView { get; set; }

        public List<ChatMessage> Messages { get; set; } = new();
    }
}