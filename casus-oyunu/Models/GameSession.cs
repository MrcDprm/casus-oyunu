using System;

namespace casus_oyunu.Models
{
    public class GameSession
    {
        public int Id { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public string Categories { get; set; } // Virgülle ayrılmış kategori adları
        public string Words { get; set; }      // Virgülle ayrılmış kelimeler
        public string Players { get; set; }    // Virgülle ayrılmış oyuncu adları
        public string SpyName { get; set; }
        public string Winner { get; set; }     // "Players" veya "Spy"
        public string EndType { get; set; }    // "TimeUp", "Vote", "Manual"
    }
} 