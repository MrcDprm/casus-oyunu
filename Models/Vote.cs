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
        public RoomParticipant? Voter { get; set; }

        public int TargetParticipantId { get; set; }
        public RoomParticipant? Target { get; set; }

        public DateTime VotedAt { get; set; } = DateTime.UtcNow;
    }
} 