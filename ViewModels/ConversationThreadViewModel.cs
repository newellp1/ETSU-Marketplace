namespace ETSU_Marketplace.ViewModels
{
    public class ConversationThreadViewModel
    {
        public int ConversationId { get; set; }
        public int ListingId { get; set; }
        public string ListingTitle { get; set; } = "";
        public string OtherUserName { get; set; } = "";
        public string OtherUserAvatar { get; set; } = "/images/default-avatar.jpg";
        public string ListingType { get; set; } = "";
        public string DetailsUrl { get; set; } = "";
        public bool IsSold { get; set; }
        public DateTime? LastMessageAtUtc { get; set; }
        public string LastMessagePreview { get; set; } = "";
    }
}