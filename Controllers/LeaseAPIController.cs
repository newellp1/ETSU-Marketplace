using ETSU_Marketplace.Models;
using ETSU_Marketplace.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ETSU_Marketplace.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LeaseAPIController : BaseAPIController<LeaseListing, ILeaseListingRepository>
{
    public LeaseAPIController(
        ILeaseListingRepository leaseRepo,
        UserManager<ApplicationUser> userManager)
        : base(leaseRepo, userManager)
    {
    }

    protected override string GetRedirectPath() => "/Manage";

    // POST: api/LeaseAPI
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] LeaseListing entity, List<IFormFile> images)
    {
        return await CreateEntity(entity, images);
    }

    // PUT: api/LeaseAPI/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromForm] LeaseListing entity, List<IFormFile> images)
    {
        if (id != entity.Id)
        {
            return BadRequest("Route ID does not match entity ID.");
        }

        return await UpdateEntity(entity, images);
    }
}