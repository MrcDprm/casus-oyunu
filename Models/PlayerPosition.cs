using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace casus_oyunu.Models
{
    public class PlayerPosition
    {
        [Key]
        public int Id { get; set; }

        public int RoomParticipantId { get; set; }
        public RoomParticipant? RoomParticipant { get; set; }

        public float X { get; set; }
        public float Y { get; set; }
        public string Color { get; set; } = "#FF0000"; // Hex color code
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
} 