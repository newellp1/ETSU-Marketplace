using ETSU_Marketplace.Models;
using ETSU_Marketplace.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ETSU_Marketplace.Controllers;

[Route("api/[controller]")]
public class LeaseAPIController : BaseAPIController<LeaseListing, ILeaseListingRepository>
{
    public LeaseAPIController(
        ILeaseListingRepository leaseRepo,
        UserManager<ApplicationUser> userManager)
        : base(leaseRepo, userManager)
    {
    }

    protected override string GetRedirectPath() => "/Manage";

    [HttpPost("create")]
    public async Task<IActionResult> Post([FromForm] LeaseListing entity, List<IFormFile> images)
    {
        return await CreateEntity(entity, images);
    }

    [HttpPost("update")]
    public async Task<IActionResult> Put([FromForm] LeaseListing entity, List<IFormFile> images)
    {
        return await UpdateEntity(entity, images);
    }
}