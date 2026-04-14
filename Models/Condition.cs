using System.ComponentModel.DataAnnotations;

public enum Condition
{
    [Display(Name = "Brand new")]
    BrandNew = 1,

    [Display(Name = "Like new")]
    LikeNew = 2,

    [Display(Name = "Used - Excellent")]
    Excellent = 3,

    [Display(Name = "Used - Good")]
    Good = 4,

    [Display(Name = "Used - Fair")]
    Fair = 5
}