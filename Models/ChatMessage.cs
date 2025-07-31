using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace casus_oyunu.Models
{
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }

        public int GameRoomId { get; set; }
        public GameRoom? GameRoom { get; set; }

        public int SenderParticipantId { get; set; }
        public RoomParticipant? Sender { get; set; }

        [Required]
        [MaxLength(500)]
        public string Message { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
} 