using System.ComponentModel.DataAnnotations;
using ETSU_Marketplace.Models;

public abstract class Listing
{
    public int Id { get; set; }

    [Required(ErrorMessage = "A title is required.")]
    [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "A description is required.")]
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
    public string Description { get; set; } = "";

    [Range(0.01, 1000000, ErrorMessage = "Price must be greater than 0.")]
    public decimal Price { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public bool IsSold { get; set; } = false;

    // One-to-many relationship with Image
    public List<Image> Images { get; set; } = new List<Image>();

    // Relationship with User
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
}