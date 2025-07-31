using System.ComponentModel.DataAnnotations;

namespace casus_oyunu.Models.ViewModels
{
    public class JoinRoomViewModel
    {
        [Required(ErrorMessage = "Oda kodu zorunludur.")]
        [StringLength(8, MinimumLength = 6, ErrorMessage = "Oda kodu 6-8 karakter olmalıdır.")]
        [Display(Name = "Oda Kodu")]
        public string RoomCode { get; set; }

        [Display(Name = "Katılım Yöntemi")]
        public string JoinMethod { get; set; } = "guest"; // "guest" veya "account"

        // Misafir katılım için
        [Display(Name = "Oyuncu Adı")]
        public string? PlayerName { get; set; }

        // Hesap oluşturma için
        [Display(Name = "Kullanıcı Adı")]
        public string? UserName { get; set; }

        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta")]
        public string? Email { get; set; }

        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string? Password { get; set; }
    }
} 