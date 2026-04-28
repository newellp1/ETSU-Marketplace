using ETSU_Marketplace.Models;
using ETSU_Marketplace.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETSU_Marketplace.Controllers
{
    /// <summary>
    /// Controller responsible for handling messaging between users.
    /// Supports viewing conversations, sending messages, and loading message history.
    /// </summary>
    [Authorize]
    [Route("Messages")]
    public class MessagesController : Controller
    {
        // Database context for accessing conversations and messages
        private readonly ApplicationDbContext _db;

        // User manager for identifying current user
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Constructor that injects database context and user manager.
        /// </summary>
        public MessagesController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        /// <summary>
        /// Displays all conversations for the current user.
        /// </summary>
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            // Load conversations with related data
            var conversations = await _db.Conversations
                .Include(c => c.Listing)
                .Include(c => c.Seller)!.ThenInclude(u => u!.Avatar)
                .Include(c => c.Buyer)!.ThenInclude(u => u!.Avatar)
                .Include(c => c.Messages)
                .Where(c => c.SellerId == userId || c.BuyerId == userId)
                .ToListAsync();

            var vm = new MessagesIndexViewModel();

            // Sort conversations by most recent message
            foreach (var c in conversations
                .OrderByDescending(x => x.Messages.Any() ? x.Messages.Max(m => m.SentAtUtc) : x.CreatedAt))
            {
                var isSeller = c.SellerId == userId;
                var otherUser = isSeller ? c.Buyer : c.Seller;

                var listingType = c.Listing is ItemListing ? "Item" : "Lease";

                // Build link to listing details
                var detailsUrl = listingType == "Item"
                    ? $"/Listings/Items/Details/{c.ListingId}"
                    : $"/Listings/Leases/Details/{c.ListingId}";

                var lastMessage = c.Messages.OrderByDescending(m => m.SentAtUtc).FirstOrDefault();

                // Add conversation summary to view model
                vm.Threads.Add(new ConversationThreadViewModel
                {
                    ConversationId = c.Id,
                    ListingId = c.ListingId,
                    ListingTitle = c.Listing?.Title ?? "Listing",
                    OtherUserName = otherUser != null
                        ? $"{otherUser.FirstName} {otherUser.LastName}"
                        : "User",
                    OtherUserAvatar = otherUser?.Avatar?.Path ?? "/images/default-avatar.jpg",
                    ListingType = listingType,
                    DetailsUrl = detailsUrl,
                    IsSold = c.Listing?.IsSold ?? false,
                    LastMessageAtUtc = lastMessage?.SentAtUtc,
                    LastMessagePreview = lastMessage?.Text ?? ""
                });
            }

            return View("Index", vm);
        }

        /// <summary>
        /// Displays details of a specific conversation with all messages.
        /// </summary>
        [HttpGet("{conversationId}")]
        public async Task<IActionResult> Details(int conversationId)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            // Load conversation with messages and users
            var conversation = await _db.Conversations
                .Include(c => c.Listing)
                .Include(c => c.Seller)!.ThenInclude(u => u!.Avatar)
                .Include(c => c.Buyer)!.ThenInclude(u => u!.Avatar)
                .Include(c => c.Messages)!.ThenInclude(m => m.Sender)
                .FirstOrDefaultAsync(c => c.Id == conversationId);

            if (conversation == null) return NotFound();

            // Ensure user is part of the conversation
            if (conversation.SellerId != userId && conversation.BuyerId != userId)
                return Forbid();

            var isSeller = conversation.SellerId == userId;
            var otherUser = isSeller ? conversation.Buyer : conversation.Seller;

            var listingType = conversation.Listing is ItemListing ? "Item" : "Lease";

            var detailsUrl = listingType == "Item"
                ? $"/Listings/Items/Details/{conversation.ListingId}"
                : $"/Listings/Leases/Details/{conversation.ListingId}";

            // Build detailed view model
            var vm = new ConversationDetailsViewModel
            {
                ConversationId = conversation.Id,
                ListingId = conversation.ListingId,
                ListingTitle = conversation.Listing?.Title ?? "Listing",
                ListingType = listingType,
                OtherUserName = otherUser != null
                    ? $"{otherUser.FirstName} {otherUser.LastName}"
                    : "User",
                OtherUserAvatar = otherUser?.Avatar?.Path ?? "/images/default-avatar.jpg",
                DetailsUrl = detailsUrl,
                IsSold = conversation.Listing?.IsSold ?? false,
                IsSellerView = isSeller,

                // Sort messages oldest to newest
                Messages = conversation.Messages.OrderBy(m => m.SentAtUtc).ToList()
            };

            return View("Details", vm);
        }

        /// <summary>
        /// Starts a new conversation for a listing or returns existing one.
        /// </summary>
        [HttpPost("start/{listingId}")]
        public async Task<IActionResult> Start(int listingId)
        {
            var buyerId = _userManager.GetUserId(User);
            if (buyerId == null) return Unauthorized();

            var listing = await _db.Listings.FirstOrDefaultAsync(l => l.Id == listingId);
            if (listing == null) return NotFound();

            // Prevent user from messaging themselves
            if (listing.UserId == buyerId)
            {
                return RedirectToAction("Details",
                    listing is ItemListing ? "ItemListings" : "LeaseListings",
                    new { id = listingId });
            }

            // Check if conversation already exists
            var existing = await _db.Conversations
                .FirstOrDefaultAsync(c => c.ListingId == listingId && c.BuyerId == buyerId);

            if (existing != null)
            {
                return RedirectToAction("Details", new { conversationId = existing.Id });
            }

            // Create new conversation
            var conversation = new Conversation
            {
                ListingId = listingId,
                SellerId = listing.UserId,
                BuyerId = buyerId
            };

            _db.Conversations.Add(conversation);
            await _db.SaveChangesAsync();

            return RedirectToAction("Details", new { conversationId = conversation.Id });
        }

        /// <summary>
        /// Returns message history for a conversation (used for AJAX/real-time updates).
        /// </summary>
        [HttpGet("conversation/{conversationId}/history")]
        public async Task<IActionResult> History(int conversationId)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var conversation = await _db.Conversations
                .Include(c => c.Messages)!.ThenInclude(m => m.Sender)
                .FirstOrDefaultAsync(c => c.Id == conversationId);

            if (conversation == null) return NotFound();

            // Ensure user is part of conversation
            if (conversation.SellerId != userId && conversation.BuyerId != userId)
                return Forbid();

            // Return messages as JSON
            var data = conversation.Messages
                .OrderBy(m => m.SentAtUtc)
                .Select(m => new
                {
                    m.Id,
                    m.Text,
                    m.SentAtUtc,
                    Sender = m.Sender != null
                        ? $"{m.Sender.FirstName} {m.Sender.LastName}"
                        : "User",
                    m.SenderId
                });

            return Ok(data);
        }
    }
}