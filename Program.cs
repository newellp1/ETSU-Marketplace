using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using ETSU_Marketplace.Hubs;
using ETSU_Marketplace.Services;
using ETSU_Marketplace.Models;
using ETSU_Marketplace;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
   options.UseSqlite(
      builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Add Identity services
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHealthChecks().ForwardToPrometheus();
builder.Services.AddScoped<IItemListingRepository, DbItemListingRepository>();
builder.Services.AddScoped<ILeaseListingRepository, DbLeaseListingRepository>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IUserRepository, DbUserRepository>();
builder.Services.AddHttpClient<GitHubIssueService>();
builder.Services.AddSignalR();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseHttpsRedirection();

app.UseHttpMethodOverride(new HttpMethodOverrideOptions
{
    FormFieldName = "_method"
});

app.UseRouting();

app.UseHttpMetrics(options =>
{
    options.ReduceStatusCodeCardinality();
});

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapMetrics();
app.MapHealthChecks("/healthz");

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

app.MapHub<MarketplaceHub>("/marketplaceHub");

app.Run();