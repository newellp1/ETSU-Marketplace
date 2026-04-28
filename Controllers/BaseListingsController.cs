using ETSU_Marketplace.Models;
using ETSU_Marketplace.Services;
using ETSU_Marketplace.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ETSU_Marketplace.Controllers;

/// <summary>
/// Base MVC controller for handling listing-related views and logic.
/// Provides shared functionality such as ownership checks and mapping entities
/// to view models for display in the UI.
/// </summary>
/// <typeparam name="TEntity">The listing entity type</typeparam>
/// <typeparam name="TRepository">Repository used for data access</typeparam>
public abstract class BaseListingsController<TEntity, TRepository> : Controller
    where TEntity : Listing
    where TRepository : IListingRepository<TEntity>
{
    // Repository used for CRUD operations on listings
    protected readonly TRepository _repository;

    // User manager used to access current user information
    protected readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    /// Constructor to initialize repository and user manager dependencies.
    /// </summary>
    /// <param name="repository">Repository for listing operations</param>
    /// <param name="userManager">User manager for authentication</param>
    protected BaseListingsController(TRepository repository, UserManager<ApplicationUser> userManager)
    {
        _repository = repository;
        _userManager = userManager;
    }

    /// <summary>
    /// Gets the currently logged-in user's ID.
    /// </summary>
    protected string? CurrentUserId => _userManager.GetUserId(User);

    /// <summary>
    /// Determines whether the current user is the owner of a listing.
    /// </summary>
    /// <param name="entity">Listing entity to check</param>
    /// <returns>True if the current user owns the listing</returns>
    protected bool IsOwner(TEntity entity)
    {
        return entity != null && entity.UserId == CurrentUserId;
    }

    /// <summary>
    /// Maps a listing entity to a view model used for displaying listing cards in the UI.
    /// </summary>
    /// <param name="entity">The listing entity</param>
    /// <param name="showOwnerActions">Whether to show edit/delete actions for the owner</param>
    /// <returns>A populated ListingCardViewModel</returns>
    protected ListingCardViewModel MapToCardViewModel(TEntity entity, bool showOwnerActions = false)
    {
        // Create and populate basic listing data
        var vm = new ListingCardViewModel
        {
            Id = entity.Id,
            Title = entity.Title,
            ShortDescription = entity.Description,
            Price = entity.Price,
            CreatedAt = entity.CreatedAt,
            IsSold = entity.IsSold,
            ShowOwnerActions = showOwnerActions,

            // Map image paths for display
            ImageUrls = entity.Images?.Select(i => i.Path).ToList() ?? new List<string>(),

            // Display user's full name or fallback if missing
            Poster = entity.User != null
                ? $"{entity.User.FirstName} {entity.User.LastName}".Trim()
                : "Unknown User",

            // Use user's avatar or fallback image
            PosterAvatar = entity.User?.Avatar?.Path ?? "/images/placeholder.png",

            PosterId = entity.UserId
        };

        // Handle additional properties for item listings
        if (entity is ItemListing item)
        {
            vm.ListingType = "Item";

            // Combine category names into a single string
            vm.CategoryLabel = item.ListingCategories != null
                ? string.Join(", ", item.ListingCategories.Select(lc => lc.Category.ToString()))
                : null;

            // Set item condition label
            vm.ConditionLabel = item.Condition.ToString();
        }
        // Handle lease listings
        else if (entity is LeaseListing)
        {
            vm.ListingType = "Lease";
        }

        return vm;
    }
}