using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace casus_oyunu.Models
{
    public class GameState
    {
        [Key]
        public int Id { get; set; }

        public int GameRoomId { get; set; }
        public GameRoom? GameRoom { get; set; }

        public bool Started { get; set; } = false;
        public bool Finished { get; set; } = false;
        public int? SpyParticipantId { get; set; }
        public string? SelectedWord { get; set; }
        public DateTime? StartedAt { get; set; }
        public int DurationSeconds { get; set; } = 480;
        public string? FinishMessage { get; set; }
    }
} 