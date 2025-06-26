using Microsoft.AspNetCore.Mvc;
using casus_oyunu.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace casus_oyunu.Controllers
{
    public class GameController : Controller
    {
        // Oyun state'i
        private static List<Player> Players = new List<Player>();
        private static List<string> SelectedCategories = new List<string>();
        private static List<string> SelectedWords = new List<string>();
        private static int SpyIndex = -1;
        private static int GameDuration = 8; // dakika
        private static int GameDurationSeconds = 480; // saniye
        private static bool GameStarted = false;
        private static string SelectedWord = string.Empty; // Seçilen tek kelime
        private static Dictionary<string, string> Votes = new Dictionary<string, string>(); // Oylar: (oy veren, oy verilen)
        private static bool GameFinished = false;
        private static string FinishMessage = string.Empty;
        private readonly GameDbContext _db;
        private static DateTime? GameStartedAt = null;

        public GameController(GameDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.Categories = CategoryData.Categories;
            ViewBag.Players = Players;
            ViewBag.GameDuration = GameDuration;
            ViewBag.SelectedCategories = SelectedCategories;
            ViewBag.SelectedWords = SelectedWords;
            ViewBag.FinishMessage = FinishMessage;
            FinishMessage = string.Empty;
            return View();
        }

        [HttpPost]
        public IActionResult AddPlayer(string playerName, string categoryNames, string wordNames, int gameDuration)
        {
            if (!string.IsNullOrWhiteSpace(playerName))
            {
                if (Players.Any(p => p.Name.Trim().ToLower() == playerName.Trim().ToLower()))
                {
                    ViewBag.Error = "Bu isimde bir oyuncu zaten var.";
                }
                else
                {
                    Players.Add(new Player { Name = playerName });
                }
            }
            // Seçili kategori, kelime ve süreyi static değişkenlere dokunmadan ViewBag ile koru
            var selectedCategories = !string.IsNullOrEmpty(categoryNames) ? categoryNames.Split(',').ToList() : SelectedCategories;
            var selectedWords = !string.IsNullOrEmpty(wordNames) ? wordNames.Split(',').ToList() : SelectedWords;
            int duration = (gameDuration >= 10 && gameDuration <= 720) ? gameDuration : GameDuration;
            ViewBag.Categories = CategoryData.Categories;
            ViewBag.Players = Players;
            ViewBag.GameDuration = duration;
            ViewBag.SelectedCategories = selectedCategories;
            ViewBag.SelectedWords = selectedWords;
            return View("Index");
        }

        [HttpPost]
        public IActionResult SetGameOptions(string categoryNames, string wordNames, int gameDuration)
        {
            // Kategori ve kelime seçimi, oyun süresi ayarı
            SelectedCategories = !string.IsNullOrEmpty(categoryNames) ? categoryNames.Split(',').ToList() : new List<string>();
            SelectedWords = !string.IsNullOrEmpty(wordNames) ? wordNames.Split(',').ToList() : new List<string>();
            // gameDuration artık saniye cinsinden geliyor
            if (gameDuration < 10) GameDuration = 10;
            else if (gameDuration > 720) GameDuration = 720;
            else GameDuration = gameDuration;
            GameDurationSeconds = GameDuration;
            ViewBag.Categories = CategoryData.Categories;
            ViewBag.Players = Players;
            ViewBag.GameDuration = GameDuration;
            ViewBag.SelectedCategories = SelectedCategories;
            ViewBag.SelectedWords = SelectedWords;
            return View("Index");
        }

        [HttpPost]
        public IActionResult StartGame()
        {
            // Oyun başlatılırken seçili kelimelerden rastgele birini seç ve casus hariç herkese dağıt
            if (SelectedWords.Count == 0 || Players.Count < 3)
            {
                ViewBag.Error = "Oyuna başlamak için seçimlerin kaydedilmesi gerek.";
                ViewBag.Categories = CategoryData.Categories;
                ViewBag.Players = Players;
                ViewBag.GameDuration = GameDuration;
                ViewBag.SelectedCategories = SelectedCategories;
                ViewBag.SelectedWords = SelectedWords;
                return View("Index");
            }
            
            GameStartedAt = DateTime.Now;
            var rnd = new System.Random();
            SpyIndex = rnd.Next(Players.Count); // Rastgele bir casus seç
            SelectedWord = SelectedWords[rnd.Next(SelectedWords.Count)]; // Tek kelime seç
            
            // Tüm oyuncuları sıfırla
            for (int i = 0; i < Players.Count; i++)
            {
                Players[i].IsSpy = false;
                Players[i].AssignedWord = "";
            }
            
            // Casus rolünü ver
            Players[SpyIndex].IsSpy = true;
            Players[SpyIndex].AssignedWord = "CASUS";
            
            // Diğer oyunculara kelimeyi ver
            for (int i = 0; i < Players.Count; i++)
            {
                if (i != SpyIndex)
                {
                    Players[i].IsSpy = false;
                    Players[i].AssignedWord = SelectedWord;
                }
            }
            
            GameStarted = true;
            return RedirectToAction("ShowCards");
        }

        [HttpGet]
        public IActionResult ShowCards()
        {
            ViewBag.Players = Players;
            ViewBag.GameDuration = GameDuration;
            ViewBag.SelectedCategories = SelectedCategories;
            ViewBag.SelectedWords = SelectedWords;
            ViewBag.SelectedWord = SelectedWord;
            ViewBag.GameStarted = GameStarted;
            ViewBag.Categories = CategoryData.Categories;
            ViewBag.Votes = Votes;
            ViewBag.SpyIndex = SpyIndex;
            return View();
        }

        [HttpPost]
        public IActionResult ResetGame()
        {
            Players.Clear();
            SelectedCategories.Clear();
            SelectedWords.Clear();
            SpyIndex = -1;
            GameDuration = 8;
            GameDurationSeconds = 480;
            GameStarted = false;
            SelectedWord = string.Empty;
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RemovePlayer(int playerIndex)
        {
            if (playerIndex >= 0 && playerIndex < Players.Count)
            {
                Players.RemoveAt(playerIndex);
            }
            ViewBag.Categories = CategoryData.Categories;
            ViewBag.Players = Players;
            ViewBag.GameDuration = GameDuration;
            ViewBag.SelectedCategories = SelectedCategories;
            ViewBag.SelectedWords = SelectedWords;
            return View("Index");
        }

        [HttpPost]
        public IActionResult FinishGame(string selectedPlayer)
        {
            GameFinished = true;
            string spyName = (SpyIndex >= 0 && SpyIndex < Players.Count) ? Players[SpyIndex].Name : "(bilinmiyor)";
            if (selectedPlayer == spyName)
            {
                FinishMessage = $"Oyuncular kazandı! Doğru tahmin: {spyName} ajandı.";
            }
            else
            {
                FinishMessage = $"Oyuncular kaybetti! Seçilen kişi ajan değildi. Ajan {spyName} idi.";
            }
            // Oyun kaydını veritabanına ekle
            var session = new GameSession
            {
                StartedAt = GameStartedAt ?? DateTime.Now,
                EndedAt = DateTime.Now,
                Categories = string.Join(",", SelectedCategories),
                Words = string.Join(",", SelectedWords),
                Players = string.Join(",", Players.Select(p => p.Name)),
                SpyName = spyName,
                Winner = (selectedPlayer == spyName) ? "Players" : "Spy",
                EndType = "Vote"
            };
            _db.GameSessions.Add(session);
            _db.SaveChanges();
            GameStartedAt = null;
            // Oyun state'ini sıfırla
            Players.Clear();
            SelectedCategories.Clear();
            SelectedWords.Clear();
            Votes.Clear();
            SpyIndex = -1;
            GameStarted = false;
            SelectedWord = string.Empty;
            GameDuration = 8;
            GameDurationSeconds = 480;
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DrawGame()
        {
            GameFinished = true;
            string spyName = (SpyIndex >= 0 && SpyIndex < Players.Count) ? Players[SpyIndex].Name : "(bilinmiyor)";
            FinishMessage = $"Kazanan olmadı. Oyun berabere. Ajan {spyName} idi.";
            // Oyun kaydını veritabanına ekle
            var session = new GameSession
            {
                StartedAt = GameStartedAt ?? DateTime.Now,
                EndedAt = DateTime.Now,
                Categories = string.Join(",", SelectedCategories),
                Words = string.Join(",", SelectedWords),
                Players = string.Join(",", Players.Select(p => p.Name)),
                SpyName = spyName,
                Winner = "None",
                EndType = "TimeUp"
            };
            _db.GameSessions.Add(session);
            _db.SaveChanges();
            GameStartedAt = null;
            // Oyun state'ini sıfırla
            Players.Clear();
            SelectedCategories.Clear();
            SelectedWords.Clear();
            Votes.Clear();
            SpyIndex = -1;
            GameStarted = false;
            SelectedWord = string.Empty;
            GameDuration = 8;
            GameDurationSeconds = 480;
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Vote(string voter, string voted)
        {
            if (!string.IsNullOrEmpty(voter) && !string.IsNullOrEmpty(voted))
            {
                if (!Votes.ContainsKey(voter))
                {
                    Votes[voter] = voted;
                }
                else
                {
                    Votes[voter] = voted;
                    TempData["VoteWarning"] = $"{voter} oyunu güncelledi.";
                }
            }
            return RedirectToAction("ShowCards");
        }

        [HttpGet]
        public IActionResult ShowVotes()
        {
            ViewBag.Votes = Votes;
            ViewBag.Players = Players;
            return View();
        }
    }
} 