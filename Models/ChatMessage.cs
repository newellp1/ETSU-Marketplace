using System.ComponentModel.DataAnnotations;

namespace ETSU_Marketplace.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }

        [Required]
        public int ConversationId { get; set; }
        public Conversation? Conversation { get; set; }

        [Required]
        public string SenderId { get; set; } = string.Empty;
        public ApplicationUser? Sender { get; set; }

        [Required]
        public string Text { get; set; } = string.Empty;

        public DateTime SentAtUtc { get; set; } = DateTime.UtcNow;
    }
}