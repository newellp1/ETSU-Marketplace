using System.ComponentModel.DataAnnotations;

namespace ETSU_Marketplace.Models
{
    public class Conversation
    {
        public int Id { get; set; }

        [Required]
        public int ListingId { get; set; }
        public Listing? Listing { get; set; }

        [Required]
        public string SellerId { get; set; } = string.Empty;
        public ApplicationUser? Seller { get; set; }

        [Required]
        public string BuyerId { get; set; } = string.Empty;
        public ApplicationUser? Buyer { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}