using System.ComponentModel.DataAnnotations;

namespace casus_oyunu.Models
{
    public enum TournamentStatus
    {
        Upcoming = 0,
        Registration = 1,
        InProgress = 2,
        Completed = 3,
        Cancelled = 4
    }

    public class Tournament
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Turnuva adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Turnuva adı 100 karakterden uzun olamaz.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Açıklama 500 karakterden uzun olamaz.")]
        public string? Description { get; set; }

        [Required]
        public TournamentStatus Status { get; set; } = TournamentStatus.Upcoming;

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? RegistrationDeadline { get; set; }

        [Range(2, 100, ErrorMessage = "Minimum katılımcı sayısı 2-100 arasında olmalıdır.")]
        public int MinParticipants { get; set; } = 4;

        [Range(2, 100, ErrorMessage = "Maksimum katılımcı sayısı 2-100 arasında olmalıdır.")]
        public int MaxParticipants { get; set; } = 32;

        [Range(0, 10000, ErrorMessage = "Giriş ücreti 0-10000 arasında olmalıdır.")]
        public decimal EntryFee { get; set; } = 0;

        [Range(0, 100000, ErrorMessage = "Ödül miktarı 0-100000 arasında olmalıdır.")]
        public decimal PrizePool { get; set; } = 0;

        [StringLength(3)]
        public string Currency { get; set; } = "TRY";

        public bool IsPremiumOnly { get; set; } = false;

        [Required]
        public int CreatorId { get; set; }
        public User? Creator { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    public class TournamentParticipant
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TournamentId { get; set; }
        public Tournament? Tournament { get; set; }

        [Required]
        public int UserId { get; set; }
        public User? User { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public bool HasPaid { get; set; } = false;

        [StringLength(50)]
        public string? PaymentTransactionId { get; set; }

        public int? FinalRank { get; set; }
        public decimal? PrizeAmount { get; set; }
    }

    public class TournamentMatch
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TournamentId { get; set; }
        public Tournament? Tournament { get; set; }

        public int? GameRoomId { get; set; }
        public GameRoom? GameRoom { get; set; }

        public int Round { get; set; } = 1;

        public int? Player1Id { get; set; }
        public User? Player1 { get; set; }

        public int? Player2Id { get; set; }
        public User? Player2 { get; set; }

        public int? WinnerId { get; set; }
        public User? Winner { get; set; }

        public DateTime? ScheduledAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public string? Result { get; set; }
    }
} 