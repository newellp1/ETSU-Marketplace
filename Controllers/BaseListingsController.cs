using ETSU_Marketplace.Models;
using ETSU_Marketplace.Services;
using ETSU_Marketplace.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ETSU_Marketplace.Controllers;

public abstract class BaseListingsController<TEntity, TRepository> : Controller
    where TEntity : Listing
    where TRepository : IListingRepository<TEntity>
{
    protected readonly TRepository _repository;
    protected readonly UserManager<ApplicationUser> _userManager;

    protected BaseListingsController(TRepository repository, UserManager<ApplicationUser> userManager)
    {
        _repository = repository;
        _userManager = userManager;
    }

    protected string? CurrentUserId => _userManager.GetUserId(User);

    protected bool IsOwner(TEntity entity)
    {
        return entity != null && entity.UserId == CurrentUserId;
    }

    protected ListingCardViewModel MapToCardViewModel(TEntity entity, bool showOwnerActions = false)
    {
        var vm = new ListingCardViewModel
        {
            Id = entity.Id,
            Title = entity.Title,
            ShortDescription = entity.Description,
            Price = entity.Price,
            CreatedAt = entity.CreatedAt,
            IsSold = entity.IsSold,
            ShowOwnerActions = showOwnerActions,

            ImageUrls = entity.Images?.Select(i => i.Path).ToList() ?? new List<string>(),

            Poster = entity.User != null
                ? $"{entity.User.FirstName} {entity.User.LastName}".Trim()
                : "Unknown User",

            PosterAvatar = entity.User?.Avatar?.Path ?? "/images/placeholder.png",

            PosterId = entity.UserId
        };

        if (entity is ItemListing item)
        {
            vm.ListingType = "Item";

            vm.CategoryLabel = item.ListingCategories != null
                ? string.Join(", ", item.ListingCategories.Select(lc => lc.Category.ToString()))
                : null;

            vm.ConditionLabel = item.Condition.ToString();
        }
        else if (entity is LeaseListing)
        {
            vm.ListingType = "Lease";
        }

        return vm;
    }
}