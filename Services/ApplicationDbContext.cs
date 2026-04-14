using ETSU_Marketplace.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ItemListing> ItemListings => Set<ItemListing>();
    public DbSet<LeaseListing> LeaseListings => Set<LeaseListing>();
    public DbSet<Listing> Listings => Set<Listing>();
    public DbSet<Image> Images => Set<Image>();
    public DbSet<FavoriteListing> FavoriteListings => Set<FavoriteListing>();
    public DbSet<ListingCategory> ListingCategories => Set<ListingCategory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Listing>(entity =>
        {
            entity.HasMany(l => l.Images)
                .WithOne(i => i.Listing)
                .HasForeignKey(i => i.ListingId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasDiscriminator<string>("ListingType")
                .HasValue<ItemListing>("ITEM")
                .HasValue<LeaseListing>("LEASE");
        });

        modelBuilder.Entity<ItemListing>(entity =>
        {
            entity.HasMany(i => i.ListingCategories)
                .WithOne(lc => lc.Listing)
                .HasForeignKey(lc => lc.ListingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ListingCategory>(entity =>
        {
            entity.HasKey(lc => new { lc.ListingId, lc.Category });
        });

        modelBuilder.Entity<ApplicationUser>()
            .HasOne(u => u.Avatar)
            .WithOne(i => i.User)
            .HasForeignKey<ApplicationUser>(u => u.AvatarId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<FavoriteListing>(entity =>
        {
            entity.HasOne(f => f.User)
                .WithMany(u => u.FavoriteListings)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(f => f.Listing)
                .WithMany()
                .HasForeignKey(f => f.ListingId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(f => new { f.UserId, f.ListingId })
                .IsUnique();
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasOne(c => c.Listing)
                .WithMany()
                .HasForeignKey(c => c.ListingId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.Seller)
                .WithMany(u => u.SellerConversations)
                .HasForeignKey(c => c.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.Buyer)
                .WithMany(u => u.BuyerConversations)
                .HasForeignKey(c => c.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(c => new { c.ListingId, c.BuyerId })
                .IsUnique();
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}