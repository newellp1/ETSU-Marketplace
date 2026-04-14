using System.ComponentModel.DataAnnotations;

namespace ETSU_Marketplace.Models
{
    public class BugReportForm
    {
        [Required]
        public string Title { get; set; } = "";

        [Required]
        public string Description { get; set; } = "";
    }
}