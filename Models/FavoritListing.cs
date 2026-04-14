using System.ComponentModel.DataAnnotations;

namespace ETSU_Marketplace.Models
{
    public class FavoriteListing
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        [Required]
        public int ListingId { get; set; }
        public Listing? Listing { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}