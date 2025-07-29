using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace casus_oyunu.Models
{
    public class Vote
    {
        [Key]
        public int Id { get; set; }

        public int GameRoomId { get; set; }
        public GameRoom? GameRoom { get; set; }

        public int VoterParticipantId { get; set; }
        public RoomParticipant? VoterParticipant { get; set; }

        public int TargetParticipantId { get; set; }
        public RoomParticipant? TargetParticipant { get; set; }

        public DateTime VotedAt { get; set; } = DateTime.UtcNow;
    }
} 