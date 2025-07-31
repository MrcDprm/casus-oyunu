using System.ComponentModel.DataAnnotations;

namespace casus_oyunu.Models
{
    public enum ThemeType
    {
        Default = 0,
        Dark = 1,
        Inspector = 2,
        Matrix = 3,
        Ocean = 4,
        Forest = 5,
        Sunset = 6
    }

    public class UserTheme
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public User? User { get; set; }

        [Required]
        public ThemeType ThemeType { get; set; } = ThemeType.Default;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
} 