using ETSU_Marketplace.Models;

public abstract class Listing
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public bool IsSold { get; set; } = false;

    //One to many relationship to Image
    public List<Image> Images { get; set; } = new List<Image>();

    //Relationship with User
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
}