using ETSU_Marketplace.Models;
using ETSU_Marketplace.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Prometheus;

namespace ETSU_Marketplace.Controllers;

/// <summary>
/// Base API controller that provides common CRRUD operations for all listing types.
/// This controller handles creating, reading, updating, deleting, and status updates
/// for listings while enforcing authentication and ownership rules.
/// </summary>
/// <typeparam name="TEntity">The listing entity type (must inherit from Listing)</typeparam>
/// <typeparam name="TRepository">Repository used to access and manage the entity</typeparam>
[EnableCors]
[Authorize]
[ApiController]
public abstract class BaseAPIController<TEntity, TRepository> : ControllerBase
    where TEntity : Listing
    where TRepository : IListingRepository<TEntity>
{
    // Repository used to interact with the database
    protected readonly TRepository _repository;

    // ASP.NET Identity user manager for accessing current user information
    protected readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    /// Constructor that initializes repository and user manager dependencies.
    /// </summary>
    /// <param name="repository">Repository for the entity</param>
    /// <param name="userManager">User manager for authentication</param>
    protected BaseAPIController(TRepository repository, UserManager<ApplicationUser> userManager)
    {
        _repository = repository;
        _userManager = userManager;
    }

    /// <summary>
    /// Creates a new listing entity and associates it with the current user.
    /// Also tracks metrics for listing creation.
    /// </summary>
    /// <param name="entity">The listing to create</param>
    /// <param name="images">Uploaded images for the listing</param>
    /// <returns>Redirects to the appropriate page after creation</returns>
    protected async Task<IActionResult> CreateEntity(TEntity entity, List<IFormFile> images)
    {
        // Get current logged-in user ID
        var userId = CurrentUserId;

        // If user is not authenticated, return unauthorized
        if (userId == null) return Unauthorized();

        // Track performance of listing creation
        using var timer = MarketplaceMetrics.ListingCreateDuration.NewTimer();

        // Create the listing in the database
        await _repository.CreateAsync(entity, images, userId);

        // Increment total listings created metric
        MarketplaceMetrics.ListingsCreated.Inc();

        return LocalRedirect(GetRedirectPath());
    }

    /// <summary>
    /// Updates an existing listing entity if the current user is the owner.
    /// </summary>
    /// <param name="entity">Updated listing data</param>
    /// <param name="images">Updated images for the listing</param>
    /// <returns>Redirects after update or error if unauthorized</returns>
    protected async Task<IActionResult> UpdateEntity(TEntity entity, List<IFormFile> images)
    {
        // Retrieve existing listing from database
        var existing = await _repository.ReadAsync(entity.Id);

        // Return error if listing does not exist
        if (existing == null) return NotFound();

        // Ensure only the owner can update the listing
        if (existing.UserId != CurrentUserId) return Forbid();

        // Update listing in database
        await _repository.UpdateAsync(entity.Id, entity, images);

        return LocalRedirect(GetRedirectPath());
    }

    /// Retrieves all listings of this type.
    /// <returns>List of all listings</returns>
    // GET: api/[controller]
    [HttpGet]
    public virtual async Task<IActionResult> GetAll()
    {
        return Ok(await _repository.ReadAllAsync());
    }

    /// <summary>
    /// Retrieves a specific listing by its ID.
    /// </summary>
    /// <param name="id">ID of the listing</param>
    /// <returns>Listing if found, otherwise NotFound</returns>
    // GET: api/[controller]/{id}
    [HttpGet("{id}")]
    public virtual async Task<IActionResult> GetById(int id)
    {
        var listing = await _repository.ReadAsync(id);

        // Return 404 if listing not found
        if (listing == null) return NotFound();

        return Ok(listing);
    }

    /// <summary>
    /// Deletes a listing if the current user is the owner.
    /// </summary>
    /// <param name="id">ID of the listing to delete</param>
    /// <returns>Redirects after deletion or error if unauthorized</returns>
    // DELETE: api/[controller]/{id}
    [HttpDelete("{id}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        var existing = await _repository.ReadAsync(id);

        // Return error if listing does not exist
        if (existing == null) return NotFound();

        // Ensure only the owner can delete the listing
        if (existing.UserId != CurrentUserId) return Forbid();

        // Delete listing from database
        await _repository.DeleteAsync(id);

        return LocalRedirect(GetRedirectPath());
    }

    /// <summary>
    /// Toggles the sold status of a listing (sold/unsold).
    /// Only the owner of the listing can perform this action.
    /// </summary>
    /// <param name="id">ID of the listing</param>
    /// <returns>Redirects after update or error if unauthorized</returns>
    // POST: api/[controller]/toggle-sold/{id}
    [HttpPost("toggle-sold/{id}")]
    public virtual async Task<IActionResult> ToggleSoldStatus(int id)
    {
        var existing = await _repository.ReadAsync(id);

        // Return error if listing does not exist
        if (existing == null) return NotFound();

        // Ensure only the owner can modify the listing
        if (existing.UserId != CurrentUserId) return Forbid();

        // Toggle sold status in database
        await _repository.ToggleSoldStatusAsync(id);

        return LocalRedirect(GetRedirectPath());
    }

    /// <summary>
    /// Gets the currently logged-in user's ID.
    /// </summary>
    protected string? CurrentUserId => _userManager.GetUserId(User);

    /// <summary>
    /// Defines the redirect path after operations.
    /// Must be implemented by derived controllers.
    /// </summary>
    protected abstract string GetRedirectPath();
}