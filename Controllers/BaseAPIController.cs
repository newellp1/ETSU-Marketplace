using ETSU_Marketplace.Models;
using ETSU_Marketplace.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Prometheus;

namespace ETSU_Marketplace.Controllers;

[EnableCors]
[Authorize]
[ApiController]
public abstract class BaseAPIController<TEntity, TRepository> : ControllerBase
    where TEntity : Listing
    where TRepository : IListingRepository<TEntity>
{
    protected readonly TRepository _repository;
    protected readonly UserManager<ApplicationUser> _userManager;

    protected BaseAPIController(TRepository repository, UserManager<ApplicationUser> userManager)
    {
        _repository = repository;
        _userManager = userManager;
    }

    protected async Task<IActionResult> CreateEntity(TEntity entity, List<IFormFile> images)
    {
        var userId = CurrentUserId;
        if (userId == null) return Unauthorized();

        using var timer = MarketplaceMetrics.ListingCreateDuration.NewTimer();

        await _repository.CreateAsync(entity, images, userId);

        MarketplaceMetrics.ListingsCreated.Inc();

        return LocalRedirect(GetRedirectPath());
    }

    protected async Task<IActionResult> UpdateEntity(TEntity entity, List<IFormFile> images)
    {
        var existing = await _repository.ReadAsync(entity.Id);

        if (existing == null) return NotFound();
        if (existing.UserId != CurrentUserId) return Forbid();

        await _repository.UpdateAsync(entity.Id, entity, images);
        return LocalRedirect(GetRedirectPath());
    }

    [HttpGet("all")]
    public virtual async Task<IActionResult> Get()
    {
        return Ok(await _repository.ReadAllAsync());
    }

    [HttpGet("one/{id}")]
    public virtual async Task<IActionResult> Get(int id)
    {
        var listing = await _repository.ReadAsync(id);
        if (listing == null) return NotFound();

        return Ok(listing);
    }

    [HttpPost("delete/{id}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        var existing = await _repository.ReadAsync(id);

        if (existing == null) return NotFound();
        if (existing.UserId != CurrentUserId) return Forbid();

        await _repository.DeleteAsync(id);
        return LocalRedirect(GetRedirectPath());
    }

    [HttpPost("toggle-sold/{id}")]
    public virtual async Task<IActionResult> ToggleSoldStatus(int id)
    {
        var existing = await _repository.ReadAsync(id);

        if (existing == null) return NotFound();
        if (existing.UserId != CurrentUserId) return Forbid();

        await _repository.ToggleSoldStatusAsync(id);
        return LocalRedirect(GetRedirectPath());
    }

    protected string? CurrentUserId => _userManager.GetUserId(User);

    protected abstract string GetRedirectPath();
}