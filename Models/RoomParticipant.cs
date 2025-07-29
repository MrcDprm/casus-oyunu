using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace casus_oyunu.Models
{
    public class RoomParticipant
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public int GameRoomId { get; set; }
        public GameRoom? GameRoom { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
} 