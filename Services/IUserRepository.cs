using System;
using ETSU_Marketplace.Models;

namespace ETSU_Marketplace.Services;

public interface IUserRepository
{
    Task<ApplicationUser?> ReadByUsernameAsync(string username);
    Task<ApplicationUser?> ReadByIdAsync(string userId);
    Task<ApplicationUser?> ReadProfileAsync(string userId);


}
