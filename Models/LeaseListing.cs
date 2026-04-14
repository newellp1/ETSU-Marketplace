public class LeaseListing : Listing
{
    public string Address {get; set;} = "";
    public DateTime LeaseStart {get; set;}
    public DateTime LeaseEnd {get; set;}
    public bool UtilitiesIncluded {get; set;}
}