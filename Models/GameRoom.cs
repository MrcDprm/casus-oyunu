using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace casus_oyunu.Models
{
    public class GameRoom
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Oda kodu zorunludur.")]
        [StringLength(8, MinimumLength = 6, ErrorMessage = "Oda kodu 6-8 karakter arasında olmalıdır.")]
        [RegularExpression(@"^[A-Z0-9]+$", ErrorMessage = "Oda kodu sadece büyük harf ve rakam içerebilir.")]
        public string RoomCode { get; set; }

        [Required(ErrorMessage = "Kurucu ID zorunludur.")]
        public int CreatorId { get; set; }
        public User? Creator { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        [StringLength(200, ErrorMessage = "Seçilen kategoriler 200 karakterden uzun olamaz.")]
        public string? SelectedCategories { get; set; }
        
        [StringLength(500, ErrorMessage = "Seçilen kelimeler 500 karakterden uzun olamaz.")]
        public string? SelectedWords { get; set; }

        [Range(60, 900, ErrorMessage = "Oyun süresi 1-15 dakika arasında olmalıdır.")]
        public int? SelectedDuration { get; set; } // saniye cinsinden

        // Navigation Properties
        public ICollection<RoomParticipant> RoomParticipants { get; set; } = new List<RoomParticipant>();
        public ICollection<GameState> GameStates { get; set; } = new List<GameState>();
        public ICollection<GameSession> GameSessions { get; set; } = new List<GameSession>();
        public ICollection<Vote> Votes { get; set; } = new List<Vote>();
        public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    }
} 