using System;
using ETSU_Marketplace.Models;
using Microsoft.EntityFrameworkCore;

namespace ETSU_Marketplace.Services;

public class DbUserRepository : IUserRepository
{
    private readonly ApplicationDbContext _db;

    public DbUserRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ApplicationUser?> ReadByUsernameAsync(string username)
    {
        return await _db.Users
            .Include(u => u.Avatar)
            .FirstOrDefaultAsync(u => u.UserName == username);
    }

    public async Task<ApplicationUser?> ReadByIdAsync(string userId)
    {
        return await _db.Users
            .Include(u => u.Avatar)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<ApplicationUser?> ReadProfileAsync(string userId)
    {
        return await _db.Users
            .Include(u => u.Avatar)
            .Include(u => u.Listings)
                .ThenInclude(l => l.Images)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

}
