using System.ComponentModel.DataAnnotations;

public class LeaseListing : Listing
{
    [Required(ErrorMessage = "An address is required.")]
    [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters.")]
    public string Address { get; set; } = "";

    [Required(ErrorMessage = "Please select a lease start date.")]
    [DataType(DataType.Date)]
    public DateTime LeaseStart { get; set; }

    [Required(ErrorMessage = "Please select a lease end date.")]
    [DataType(DataType.Date)]
    public DateTime LeaseEnd { get; set; }

    public bool UtilitiesIncluded { get; set; }
}