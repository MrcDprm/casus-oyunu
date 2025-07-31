using System.ComponentModel.DataAnnotations;

namespace casus_oyunu.Models.ViewModels
{
    public class CreateRoomViewModel
    {
        [Required(ErrorMessage = "Oyun süresi zorunludur.")]
        [Range(420, 840, ErrorMessage = "Oyun süresi 7-14 dakika arasında olmalıdır.")]
        [Display(Name = "Oyun Süresi (saniye)")]
        public int DurationSeconds { get; set; } = 480; // 8 dakika varsayılan
    }
} 