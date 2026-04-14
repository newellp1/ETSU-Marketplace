using System.Text.Json.Serialization;

namespace ETSU_Marketplace.Models;

public class Image
{
    public int Id {get; set;}
    public string Path {get; set;} = "";
    public int? ListingId {get; set;}
    [JsonIgnore]
    public Listing? Listing {get; set;}
    public ApplicationUser? User { get; set; }
}