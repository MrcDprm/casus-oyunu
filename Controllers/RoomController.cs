using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using casus_oyunu.Data;
using casus_oyunu.Models;
using casus_oyunu.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace casus_oyunu.Controllers
{
    [Authorize]
    public class RoomController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RoomController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Create()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userName = User.Identity.Name;
            
            ViewBag.UserName = userName;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRoom(CreateRoomViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Create", model);
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            // Oda kodu oluştur (6 karakter, benzersiz)
            string roomCode;
            do
            {
                roomCode = GenerateRoomCode();
            } while (await _context.GameRooms.AnyAsync(r => r.RoomCode == roomCode));
            
            var room = new GameRoom
            {
                RoomCode = roomCode,
                CreatorId = userId,
                SelectedDuration = model.DurationSeconds,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            
            _context.GameRooms.Add(room);
            await _context.SaveChangesAsync();
            
            // Kurucu odaya katılımcı olarak eklenir
            var participant = new RoomParticipant
            {
                UserId = userId,
                GameRoomId = room.Id,
                JoinedAt = DateTime.UtcNow
            };
            _context.RoomParticipants.Add(participant);
            await _context.SaveChangesAsync();
            
            return RedirectToAction("Lobby", new { code = roomCode });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Join(string roomCode = null)
        {
            ViewBag.RoomCode = roomCode;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> JoinRoom(JoinRoomViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.RoomCode = model.RoomCode;
                return View("Join", model);
            }

            var room = await _context.GameRooms
                .FirstOrDefaultAsync(r => r.RoomCode == model.RoomCode && r.IsActive);
            
            if (room == null)
            {
                ModelState.AddModelError("RoomCode", "Böyle bir oda bulunamadı veya oda aktif değil.");
                ViewBag.RoomCode = model.RoomCode;
                return View("Join", model);
            }
            
            User user;
            if (model.JoinMethod == "guest")
            {
                // Misafir katılım
                if (string.IsNullOrWhiteSpace(model.PlayerName))
                {
                    ModelState.AddModelError("PlayerName", "Misafir katılım için isim gereklidir.");
                    ViewBag.RoomCode = model.RoomCode;
                    return View("Join", model);
                }
                
                // Aynı isimle katılımı engelle
                var existing = await _context.RoomParticipants
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.GameRoomId == room.Id && p.User.UserName == model.PlayerName);
                
                if (existing != null)
                {
                    ModelState.AddModelError("PlayerName", "Bu isimle zaten bir oyuncu var.");
                    ViewBag.RoomCode = model.RoomCode;
                    return View("Join", model);
                }
                
                // Geçici kullanıcı oluştur
                user = new User
                {
                    UserName = model.PlayerName,
                    Email = $"{model.PlayerName}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}@guest.local",
                    PasswordHash = "guest",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Hesap oluşturma
                if (string.IsNullOrWhiteSpace(model.UserName) || 
                    string.IsNullOrWhiteSpace(model.Email) || 
                    string.IsNullOrWhiteSpace(model.Password))
                {
                    ModelState.AddModelError("", "Hesap oluşturmak için tüm alanları doldurun.");
                    ViewBag.RoomCode = model.RoomCode;
                    return View("Join", model);
                }
                
                if (await _context.Users.AnyAsync(u => u.UserName == model.UserName))
                {
                    ModelState.AddModelError("UserName", "Bu kullanıcı adı zaten alınmış.");
                    ViewBag.RoomCode = model.RoomCode;
                    return View("Join", model);
                }
                
                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Bu e-posta zaten kayıtlı.");
                    ViewBag.RoomCode = model.RoomCode;
                    return View("Join", model);
                }
                
                user = new User
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    PasswordHash = HashPassword(model.Password),
                    CreatedAt = DateTime.UtcNow
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                
                // Otomatik giriş yap
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName)
                };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                
                TempData["LoginSuccess"] = $"Hoş geldiniz, {user.UserName}! Hesabınız oluşturuldu ve oyuna katıldınız.";
            }
            
            // Odaya katılımcı olarak ekle
            var participant = new RoomParticipant
            {
                UserId = user.Id,
                GameRoomId = room.Id,
                JoinedAt = DateTime.UtcNow
            };
            _context.RoomParticipants.Add(participant);
            await _context.SaveChangesAsync();
            
            return RedirectToAction("Lobby", new { code = model.RoomCode });
        }

        [HttpGet]
        public async Task<IActionResult> Lobby(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return RedirectToAction("Join");
            
            var room = await _context.GameRooms
                .Include(r => r.Creator)
                .FirstOrDefaultAsync(r => r.RoomCode == code);
            
            if (room == null)
                return RedirectToAction("Join");
            
            var participants = await _context.RoomParticipants
                .Where(p => p.GameRoomId == room.Id)
                .Include(p => p.User)
                .OrderBy(p => p.JoinedAt)
                .ToListAsync();
            
            // Aktif oyun session'ı
            var gameSession = await _context.GameSessions
                .FirstOrDefaultAsync(s => s.GameRoomId == room.Id && !s.GameFinished);
            
            // Oyuncu pozisyonları
            var playerPositions = await _context.PlayerPositions
                .Where(pp => participants.Select(p => p.Id).Contains(pp.RoomParticipantId))
                .ToListAsync();
            
            // Sohbet mesajları (son 50 mesaj)
            var chatMessages = await _context.ChatMessages
                .Where(cm => cm.GameRoomId == room.Id)
                .Include(cm => cm.Sender)
                .ThenInclude(s => s.User)
                .OrderByDescending(cm => cm.SentAt)
                .Take(50)
                .OrderBy(cm => cm.SentAt)
                .ToListAsync();
            
            ViewBag.Room = room;
            ViewBag.Participants = participants;
            ViewBag.GameSession = gameSession;
            ViewBag.PlayerPositions = playerPositions;
            ViewBag.ChatMessages = chatMessages;
            
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> SelectWords(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return RedirectToAction("Lobby", new { code });
            
            var room = await _context.GameRooms.FirstOrDefaultAsync(r => r.RoomCode == code);
            if (room == null)
                return RedirectToAction("Lobby", new { code });
            
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (room.CreatorId != userId)
                return RedirectToAction("Lobby", new { code });
            
            // Kategoriler ve kelimeler
            var categories = casus_oyunu.Models.CategoryData.Categories;
            ViewBag.Room = room;
            ViewBag.Categories = categories;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectWords(string code, List<string> selectedCategories, List<string> selectedWords, int? selectedDurationMinutes)
        {
            if (string.IsNullOrWhiteSpace(code))
                return RedirectToAction("Lobby", new { code });
            
            var room = await _context.GameRooms.FirstOrDefaultAsync(r => r.RoomCode == code);
            if (room == null)
                return RedirectToAction("Lobby", new { code });
            
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (room.CreatorId != userId)
                return RedirectToAction("Lobby", new { code });
            
            if (selectedCategories == null || selectedCategories.Count < 2)
            {
                TempData["SelectError"] = "En az 2 kategori seçmelisiniz.";
                return RedirectToAction("SelectWords", new { code });
            }
            
            if (selectedWords == null || selectedWords.Count < 10)
            {
                TempData["SelectError"] = "En az 10 kelime seçmelisiniz.";
                return RedirectToAction("SelectWords", new { code });
            }
            
            if (selectedDurationMinutes == null || selectedDurationMinutes < 7 || selectedDurationMinutes > 14)
            {
                TempData["SelectError"] = "Oyun süresi 7-14 dakika arasında olmalı.";
                return RedirectToAction("SelectWords", new { code });
            }
            
            room.SelectedCategories = string.Join(",", selectedCategories);
            room.SelectedWords = string.Join(",", selectedWords);
            room.SelectedDuration = selectedDurationMinutes.Value * 60;
            
            _context.GameRooms.Update(room);
            await _context.SaveChangesAsync();
            
            return RedirectToAction("Lobby", new { code });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartGame(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return RedirectToAction("Lobby", new { code });
            
            var room = await _context.GameRooms
                .Include(r => r.Creator)
                .FirstOrDefaultAsync(r => r.RoomCode == code && r.IsActive);
            
            if (room == null)
                return RedirectToAction("Lobby", new { code });
            
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (room.CreatorId != userId)
                return RedirectToAction("Lobby", new { code });
            
            // En az 4 oyuncu kontrolü
            var participants = await _context.RoomParticipants
                .Where(p => p.GameRoomId == room.Id)
                .ToListAsync();
            
            if (participants.Count < 4)
            {
                TempData["GameError"] = "Oyun başlatmak için en az 4 oyuncu gerekli.";
                return RedirectToAction("Lobby", new { code });
            }
            
            // Kategori ve kelime seçimi kontrolü
            if (string.IsNullOrEmpty(room.SelectedCategories) || 
                room.SelectedCategories.Split(',').Length < 2 ||
                string.IsNullOrEmpty(room.SelectedWords) ||
                room.SelectedWords.Split(',').Length < 10)
            {
                TempData["GameError"] = "En az 2 kategori ve 10 kelime seçilmelidir.";
                return RedirectToAction("SelectWords", new { code });
            }
            
            // Aktif oyun session'ı var mı kontrol et
            var existingSession = await _context.GameSessions
                .FirstOrDefaultAsync(s => s.GameRoomId == room.Id && !s.GameFinished);
            
            if (existingSession != null)
            {
                TempData["GameError"] = "Zaten aktif bir oyun var.";
                return RedirectToAction("Lobby", new { code });
            }
            
            // Rastgele casus seç
            var random = new Random();
            var spyIndex = random.Next(participants.Count);
            var spyParticipant = participants[spyIndex];
            
            // Kelime seç
            var words = room.SelectedWords.Split(',');
            var selectedWord = words[random.Next(words.Length)];
            
            // Oyun session'ı oluştur
            var gameSession = new GameSession
            {
                GameRoomId = room.Id,
                StartedAt = DateTime.UtcNow,
                DurationSeconds = room.SelectedDuration ?? 480,
                CurrentQuestionerId = participants[random.Next(participants.Count)].Id,
                SpyParticipantId = spyParticipant.Id,
                VotingEnabled = false,
                GameFinished = false
            };
            
            _context.GameSessions.Add(gameSession);
            await _context.SaveChangesAsync();
            
            // Oyuncu rolleri oluştur
            foreach (var participant in participants)
            {
                var role = new PlayerRole
                {
                    RoomParticipantId = participant.Id,
                    IsSpy = participant.Id == spyParticipant.Id,
                    AssignedWord = participant.Id == spyParticipant.Id ? "CASUS" : selectedWord
                };
                _context.PlayerRoles.Add(role);
            }
            
            // Oyuncu pozisyonları oluştur
            var playerColors = new[] { "#FF0000", "#00FF00", "#0000FF", "#FFFF00", "#FF00FF", "#00FFFF", "#FF8000", "#8000FF" };
            
            for (int i = 0; i < participants.Count; i++)
            {
                var position = new PlayerPosition
                {
                    RoomParticipantId = participants[i].Id,
                    X = 100 + (i * 50),
                    Y = 100 + (i * 50),
                    Color = playerColors[i % playerColors.Length],
                    LastUpdated = DateTime.UtcNow
                };
                _context.PlayerPositions.Add(position);
            }
            
            await _context.SaveChangesAsync();
            
            return RedirectToAction("Game2D", new { code });
        }

        [HttpGet]
        public async Task<IActionResult> Game2D(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return RedirectToAction("Join");
            
            var room = await _context.GameRooms
                .Include(r => r.Creator)
                .FirstOrDefaultAsync(r => r.RoomCode == code);
            
            if (room == null)
                return RedirectToAction("Join");
            
            var gameSession = await _context.GameSessions
                .FirstOrDefaultAsync(s => s.GameRoomId == room.Id && !s.GameFinished);
            
            if (gameSession == null)
                return RedirectToAction("Lobby", new { code });
            
            var participants = await _context.RoomParticipants
                .Where(p => p.GameRoomId == room.Id)
                .Include(p => p.User)
                .Select(p => new { 
                    p.Id, 
                    p.UserId, 
                    User = new { p.User.UserName, p.User.Email }
                })
                .ToListAsync();
            
            var playerPositions = await _context.PlayerPositions
                .Where(pp => participants.Select(p => p.Id).Contains(pp.RoomParticipantId))
                .Select(pp => new { pp.RoomParticipantId, pp.X, pp.Y, pp.Color })
                .ToListAsync();
            
            // Eğer oyuncuların pozisyonu yoksa, varsayılan pozisyonlar oluştur
            if (!playerPositions.Any())
            {
                var colors = new[] { "#FF0000", "#00FF00", "#0000FF", "#FFFF00", "#FF00FF", "#00FFFF", "#FF8800", "#8800FF" };
                var random = new Random();
                
                for (int i = 0; i < participants.Count; i++)
                {
                    var participant = participants[i];
                    var position = new PlayerPosition
                    {
                        RoomParticipantId = participant.Id,
                        X = 100 + (i * 100), // Yatay dağılım
                        Y = 300 + (i % 2 * 100), // Dikey dağılım
                        Color = colors[i % colors.Length],
                        LastUpdated = DateTime.UtcNow
                    };
                    _context.PlayerPositions.Add(position);
                }
                await _context.SaveChangesAsync();
                
                // Güncellenmiş pozisyonları al
                playerPositions = await _context.PlayerPositions
                    .Where(pp => participants.Select(p => p.Id).Contains(pp.RoomParticipantId))
                    .Select(pp => new { pp.RoomParticipantId, pp.X, pp.Y, pp.Color })
                    .ToListAsync();
            }
            
            ViewBag.Room = new { room.Id, room.RoomCode };
            ViewBag.GameSession = new { 
                gameSession?.Id, 
                gameSession?.StartedAt, 
                gameSession?.DurationSeconds, 
                gameSession?.CurrentQuestion, 
                gameSession?.CurrentAnswer, 
                gameSession?.CurrentQuestionerId, 
                gameSession?.CurrentAnswererId, 
                gameSession?.SpyParticipantId, 
                gameSession?.VotingEnabled, 
                gameSession?.GameFinished, 
                gameSession?.Winner 
            };
            ViewBag.Participants = participants;
            ViewBag.PlayerPositions = playerPositions;
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AskQuestion(string code, string question, int targetParticipantId)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Json(new { success = false, message = "Oda kodu gerekli" });
            
            var room = await _context.GameRooms.FirstOrDefaultAsync(r => r.RoomCode == code);
            if (room == null)
                return Json(new { success = false, message = "Oda bulunamadı" });
            
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var participant = await _context.RoomParticipants
                .FirstOrDefaultAsync(p => p.GameRoomId == room.Id && p.UserId == userId);
            
            if (participant == null)
                return Json(new { success = false, message = "Oyuncu bulunamadı" });
            
            var gameSession = await _context.GameSessions
                .FirstOrDefaultAsync(s => s.GameRoomId == room.Id && !s.GameFinished);
            
            if (gameSession == null)
                return Json(new { success = false, message = "Aktif oyun bulunamadı" });
            
            // Sadece sıradaki kişi soru sorabilir
            if (gameSession.CurrentQuestionerId != participant.Id)
            {
                return Json(new { success = false, message = "Sıra sizde değil." });
            }
            
            // Hedef oyuncu geçerli mi?
            var targetParticipant = await _context.RoomParticipants
                .FirstOrDefaultAsync(p => p.Id == targetParticipantId && p.GameRoomId == room.Id);
            
            if (targetParticipant == null)
            {
                return Json(new { success = false, message = "Geçersiz hedef oyuncu." });
            }
            
            // Soru ve cevap oluştur
            gameSession.CurrentQuestion = question;
            gameSession.CurrentAnswererId = targetParticipantId;
            gameSession.CurrentAnswer = GenerateAnswer(question); // Basit bir cevap üretme sistemi
            
            _context.GameSessions.Update(gameSession);
            await _context.SaveChangesAsync();
            
            return Json(new { success = true, answer = gameSession.CurrentAnswer });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Vote(string code, int targetParticipantId)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Json(new { success = false, message = "Oda kodu gerekli" });
            
            var room = await _context.GameRooms.FirstOrDefaultAsync(r => r.RoomCode == code);
            if (room == null)
                return Json(new { success = false, message = "Oda bulunamadı" });
            
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var participant = await _context.RoomParticipants
                .FirstOrDefaultAsync(p => p.GameRoomId == room.Id && p.UserId == userId);
            
            if (participant == null)
                return Json(new { success = false, message = "Oyuncu bulunamadı" });
            
            // Oyuncu zaten oy kullandı mı?
            var existingVote = await _context.Votes
                .FirstOrDefaultAsync(v => v.GameRoomId == room.Id && v.VoterParticipantId == participant.Id);
            
            if (existingVote != null)
            {
                return Json(new { success = false, message = "Zaten oy kullandınız." });
            }
            
            // Hedef katılımcı geçerli mi?
            var target = await _context.RoomParticipants
                .FirstOrDefaultAsync(p => p.Id == targetParticipantId && p.GameRoomId == room.Id);
            
            if (target == null)
            {
                return Json(new { success = false, message = "Geçersiz oyuncu seçimi." });
            }
            
            var vote = new Vote
            {
                GameRoomId = room.Id,
                VoterParticipantId = participant.Id,
                TargetParticipantId = target.Id,
                VotedAt = DateTime.UtcNow
            };
            
            _context.Votes.Add(vote);
            await _context.SaveChangesAsync();
            
            // Oylama tamamlandı mı kontrol et
            var totalVotes = await _context.Votes.CountAsync(v => v.GameRoomId == room.Id);
            var totalParticipants = await _context.RoomParticipants.CountAsync(p => p.GameRoomId == room.Id);
            
            if (totalVotes >= totalParticipants)
            {
                await FinishGameInternal(code, room.Id);
                return Json(new { success = true, message = "Oyun bitti!" });
            }
            
            return Json(new { success = true, message = "Oyunuz kaydedildi." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinishByTimeout(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return RedirectToAction("Game2D", new { code });
            
            var room = await _context.GameRooms.FirstOrDefaultAsync(r => r.RoomCode == code);
            if (room == null)
                return RedirectToAction("Game2D", new { code });
            
            var gameSession = await _context.GameSessions
                .FirstOrDefaultAsync(s => s.GameRoomId == room.Id && !s.GameFinished);
            
            if (gameSession == null)
                return RedirectToAction("Game2D", new { code });
            
            // Süre doldu, casus kazandı
            gameSession.GameFinished = true;
            gameSession.Winner = "spy";
            gameSession.EndedAt = DateTime.UtcNow;
            
            _context.GameSessions.Update(gameSession);
            await _context.SaveChangesAsync();
            
            return RedirectToAction("Lobby", new { code });
        }

        private async Task<IActionResult> FinishGameInternal(string code, int roomId)
        {
            var gameSession = await _context.GameSessions
                .FirstOrDefaultAsync(s => s.GameRoomId == roomId && !s.GameFinished);
            
            if (gameSession == null)
                return RedirectToAction("Lobby", new { code });
            
            var votes = await _context.Votes.Where(v => v.GameRoomId == roomId).ToListAsync();
            var participants = await _context.RoomParticipants.Where(p => p.GameRoomId == roomId).ToListAsync();
            
            var spyParticipant = participants.FirstOrDefault(p => p.Id == gameSession.SpyParticipantId);
            var voteCounts = votes.GroupBy(v => v.TargetParticipantId).ToDictionary(g => g.Key, g => g.Count());
            var mostVotedParticipantId = voteCounts.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key;
            var mostVotedParticipant = participants.FirstOrDefault(p => p.Id == mostVotedParticipantId);
            
            string result;
            if (mostVotedParticipant?.Id == spyParticipant?.Id)
            {
                result = "players"; // Oyuncular kazandı
            }
            else
            {
                result = "spy"; // Casus kazandı
            }
            
            gameSession.GameFinished = true;
            gameSession.Winner = result;
            gameSession.EndedAt = DateTime.UtcNow;
            
            _context.GameSessions.Update(gameSession);
            await _context.SaveChangesAsync();
            
            return RedirectToAction("Lobby", new { code });
        }

        private string GenerateRoomCode()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        private string GenerateAnswer(string question)
        {
            // Basit bir cevap üretme sistemi
            var answers = new[] { "Evet", "Hayır", "Belki", "Bilmiyorum", "Muhtemelen", "Kesinlikle" };
            var random = new Random();
            return answers[random.Next(answers.Length)];
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePosition(string roomCode, float x, float y)
        {
            if (string.IsNullOrWhiteSpace(roomCode))
                return Json(new { success = false, message = "Oda kodu gerekli" });
            
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var room = await _context.GameRooms.FirstOrDefaultAsync(r => r.RoomCode == roomCode);
            
            if (room == null)
                return Json(new { success = false, message = "Oda bulunamadı" });

            var participant = await _context.RoomParticipants
                .FirstOrDefaultAsync(p => p.GameRoomId == room.Id && p.UserId == userId);
            
            if (participant == null)
                return Json(new { success = false, message = "Oyuncu bulunamadı" });
            
            var position = await _context.PlayerPositions
                .FirstOrDefaultAsync(p => p.RoomParticipantId == participant.Id);
            
            if (position == null)
            {
                position = new PlayerPosition
                {
                    RoomParticipantId = participant.Id,
                    X = x,
                    Y = y,
                    Color = "#FF0000",
                    LastUpdated = DateTime.UtcNow
                };
                _context.PlayerPositions.Add(position);
            }
            else
            {
                position.X = x;
                position.Y = y;
                position.LastUpdated = DateTime.UtcNow;
                _context.PlayerPositions.Update(position);
            }
            
            await _context.SaveChangesAsync();
            
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(string roomCode, string message)
        {
            if (string.IsNullOrWhiteSpace(roomCode) || string.IsNullOrWhiteSpace(message))
                return Json(new { success = false, message = "Oda kodu ve mesaj gerekli" });
            
            var room = await _context.GameRooms.FirstOrDefaultAsync(r => r.RoomCode == roomCode);
            if (room == null)
                return Json(new { success = false, message = "Oda bulunamadı" });
            
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var participant = await _context.RoomParticipants
                .FirstOrDefaultAsync(p => p.GameRoomId == room.Id && p.UserId == userId);
            
            if (participant == null)
                return Json(new { success = false, message = "Oyuncu bulunamadı" });
            
            var chatMessage = new ChatMessage
            {
                GameRoomId = room.Id,
                SenderParticipantId = participant.Id,
                Message = message,
                SentAt = DateTime.UtcNow
            };
            
            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();
            
            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> ShowRole(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return RedirectToAction("Join");
            
            var room = await _context.GameRooms.FirstOrDefaultAsync(r => r.RoomCode == code);
            if (room == null)
                return RedirectToAction("Join");
            
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var currentParticipant = await _context.RoomParticipants
                .FirstOrDefaultAsync(p => p.GameRoomId == room.Id && p.UserId == userId);
            
            if (currentParticipant == null)
                return RedirectToAction("Join");
            
            // Sadece kendi rolünü göster
            var playerRole = await _context.PlayerRoles
                .FirstOrDefaultAsync(pr => pr.RoomParticipantId == currentParticipant.Id);
            
            if (playerRole == null)
            {
                TempData["RoleError"] = "Rol henüz atanmamış.";
                return RedirectToAction("Game2D", new { code });
            }
            
            // Kendi bilgilerini al
            var currentParticipantWithUser = await _context.RoomParticipants
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == currentParticipant.Id);
            
            ViewBag.Role = playerRole;
            ViewBag.TargetPlayer = currentParticipantWithUser?.User?.UserName ?? "Bilinmeyen Oyuncu";
            ViewBag.RoomCode = code;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetGameState(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Json(new { success = false, message = "Oda kodu gerekli" });
            
            var room = await _context.GameRooms.FirstOrDefaultAsync(r => r.RoomCode == code);
            if (room == null)
                return Json(new { success = false, message = "Oda bulunamadı" });
            
            var gameSession = await _context.GameSessions
                .FirstOrDefaultAsync(s => s.GameRoomId == room.Id && !s.GameFinished);
            
            if (gameSession == null)
                return Json(new { success = false, message = "Aktif oyun bulunamadı" });
            
            var participantIds = await _context.RoomParticipants
                .Where(p => p.GameRoomId == room.Id)
                .Select(p => p.Id)
                .ToListAsync();
            
            var playerPositions = await _context.PlayerPositions
                .Where(pp => participantIds.Contains(pp.RoomParticipantId))
                .Select(pp => new { pp.RoomParticipantId, pp.X, pp.Y, pp.Color })
                .ToListAsync();
            
            // Chat mesajlarını al
            var chatMessages = await _context.ChatMessages
                .Where(cm => cm.GameRoomId == room.Id)
                .Include(cm => cm.Sender)
                .ThenInclude(s => s.User)
                .OrderByDescending(cm => cm.SentAt)
                .Take(50)
                .OrderBy(cm => cm.SentAt)
                .Select(cm => new { 
                    message = cm.Message, 
                    senderName = cm.Sender.User.UserName,
                    sentAt = cm.SentAt
                })
                .ToListAsync();
            
            return Json(new { 
                success = true, 
                gameSession = new {
                    gameSession.Id,
                    gameSession.StartedAt,
                    gameSession.DurationSeconds,
                    gameSession.CurrentQuestion,
                    gameSession.CurrentAnswer,
                    gameSession.CurrentQuestionerId,
                    gameSession.CurrentAnswererId,
                    gameSession.SpyParticipantId,
                    gameSession.VotingEnabled,
                    gameSession.GameFinished,
                    gameSession.Winner
                },
                playerPositions = playerPositions,
                chatMessages = chatMessages
            });
        }
    }
} 