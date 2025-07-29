using System.ComponentModel.DataAnnotations;

namespace casus_oyunu.Models
{
    public enum SubscriptionType
    {
        Free = 0,
        Basic = 1,
        Premium = 2,
        Pro = 3
    }

    public class UserSubscription
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public User? User { get; set; }

        [Required]
        public SubscriptionType Type { get; set; } = SubscriptionType.Free;

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [StringLength(100)]
        public string? PaymentMethod { get; set; }

        [StringLength(50)]
        public string? TransactionId { get; set; }

        public decimal? Amount { get; set; }

        [StringLength(3)]
        public string? Currency { get; set; } = "TRY";

        public bool IsActive => StartDate.HasValue && EndDate.HasValue && DateTime.UtcNow >= StartDate.Value && DateTime.UtcNow <= EndDate.Value;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
} 