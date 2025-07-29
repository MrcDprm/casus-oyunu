using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace casus_oyunu.Models
{
    public class PlayerRole
    {
        [Key]
        public int Id { get; set; }

        public int RoomParticipantId { get; set; }
        public RoomParticipant? RoomParticipant { get; set; }

        [Required]
        public bool IsSpy { get; set; }

        public string? AssignedWord { get; set; }
    }
} 