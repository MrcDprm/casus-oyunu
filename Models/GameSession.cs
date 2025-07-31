using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace casus_oyunu.Models
{
    public class GameSession
    {
        [Key]
        public int Id { get; set; }

        public int GameRoomId { get; set; }
        public GameRoom? GameRoom { get; set; }

        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public int DurationSeconds { get; set; }
        
        public string? CurrentQuestion { get; set; }
        public string? CurrentAnswer { get; set; }
        public int? CurrentQuestionerId { get; set; }
        public int? CurrentAnswererId { get; set; }
        public int? SpyParticipantId { get; set; }
        
        public bool VotingEnabled { get; set; } = false;
        public bool GameFinished { get; set; } = false;
        public string? Winner { get; set; } // "spy" veya "players"
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 