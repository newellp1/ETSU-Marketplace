using ETSU_Marketplace.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ETSU_Marketplace.Hubs
{
    [Authorize]
    public class MarketplaceHub : Hub
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public MarketplaceHub(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task JoinConversation(int conversationId)
        {
            if (Context.User == null) return;

            var user = await _userManager.GetUserAsync(Context.User);
            if (user == null) return;

            var conversation = await _db.Conversations.FirstOrDefaultAsync(c => c.Id == conversationId);
            if (conversation == null) return;

            if (conversation.SellerId != user.Id && conversation.BuyerId != user.Id)
                return;

            await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation-{conversationId}");
        }

        public async Task SendConversationMessage(int conversationId, string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            if (Context.User == null) return;

            var user = await _userManager.GetUserAsync(Context.User);
            if (user == null) return;

            var conversation = await _db.Conversations
                .Include(c => c.Listing)
                .FirstOrDefaultAsync(c => c.Id == conversationId);

            if (conversation == null) return;

            if (conversation.SellerId != user.Id && conversation.BuyerId != user.Id)
                return;

            var msg = new ChatMessage
            {
                ConversationId = conversationId,
                SenderId = user.Id,
                Text = text.Trim(),
                SentAtUtc = DateTime.UtcNow
            };

            _db.ChatMessages.Add(msg);
            await _db.SaveChangesAsync();

            await Clients.Group($"conversation-{conversationId}")
                .SendAsync("ReceiveConversationMessage", new
                {
                    msg.Id,
                    msg.Text,
                    msg.SentAtUtc,
                    Sender = $"{user.FirstName} {user.LastName}",
                    SenderId = user.Id
                });
        }
    }
}