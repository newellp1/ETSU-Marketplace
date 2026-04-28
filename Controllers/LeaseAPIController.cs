using ETSU_Marketplace.Models;
using ETSU_Marketplace.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ETSU_Marketplace.Controllers;

/// <summary>
/// API controller for handling lease listing operations.
/// Provides endpoints for creating and updating lease listings.
/// Inherits shared CRUD functionality from BaseAPIController.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class LeaseAPIController : BaseAPIController<LeaseListing, ILeaseListingRepository>
{
    /// <summary>
    /// Constructor that injects repository and user manager.
    /// </summary>
    /// <param name="leaseRepo">Repository for lease listings</param>
    /// <param name="userManager">ASP.NET Identity user manager</param>
    public LeaseAPIController(
        ILeaseListingRepository leaseRepo,
        UserManager<ApplicationUser> userManager)
        : base(leaseRepo, userManager)
    {
    }

    /// <summary>
    /// Specifies redirect path after create/update/delete actions.
    /// </summary>
    protected override string GetRedirectPath() => "/Manage";

    /// <summary>
    /// Creates a new lease listing.
    /// Accepts form data including listing details and uploaded images.
    /// </summary>
    /// <param name="entity">Lease listing data</param>
    /// <param name="images">Uploaded images</param>
    /// <returns>Redirect after successful creation</returns>
    // POST: api/LeaseAPI
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromForm] LeaseListing entity,
        List<IFormFile> images)
    {
        // Call base method to handle creation logic
        return await CreateEntity(entity, images);
    }

    /// <summary>
    /// Updates an existing lease listing.
    /// Validates ID and applies updates including images.
    /// </summary>
    /// <param name="id">Listing ID from route</param>
    /// <param name="entity">Updated lease listing data</param>
    /// <param name="images">Updated images</param>
    /// <returns>Redirect after successful update</returns>
    // PUT: api/LeaseAPI/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        [FromForm] LeaseListing entity,
        List<IFormFile> images)
    {
        // Ensure route ID matches entity ID
        if (id != entity.Id)
        {
            return BadRequest("Route ID does not match entity ID.");
        }

        // Call base method to handle update logic
        return await UpdateEntity(entity, images);
    }
}