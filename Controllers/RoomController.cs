using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using casus_oyunu.Data;
using casus_oyunu.Models;
using casus_oyunu.Services;
using Microsoft.AspNetCore.SignalR;
using casus_oyunu.Hubs;
using System.Security.Claims;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace casus_oyunu.Controllers
{
    [Authorize]
    public class RoomController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<GameHub> _hubContext;
        private readonly ICacheService _cacheService;

        public RoomController(ApplicationDbContext context, IHubContext<GameHub> hubContext, ICacheService cacheService)
        {
            _context = context;
            _hubContext = hubContext;
            _cacheService = cacheService;
        }

        [HttpGet]
        [Authorize]
        public IActionResult Create()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userName = User.Identity.Name;
            // Oda kodu ve linki henüz yok, dummy göster
            ViewBag.RoomCode = "Oluşturulacak";
            ViewBag.RoomLink = Request.Scheme + "://" + Request.Host + Url.Action("Join");
            ViewBag.UserName = userName;
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRoom()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var creator = await _context.Users.FindAsync(userId);

            string roomCode;
            do
            {
                roomCode = GenerateRoomCode();
            } while (await _context.GameRooms.AnyAsync(r => r.RoomCode == roomCode));

            var room = new GameRoom
            {
                RoomCode = roomCode,
                CreatorId = creator.Id
            };
            _context.GameRooms.Add(room);
            await _context.SaveChangesAsync();

            // Kurucu odaya katılımcı olarak eklenir
            var participant = new RoomParticipant
            {
                UserId = creator.Id,
                GameRoomId = room.Id
            };
            _context.RoomParticipants.Add(participant);
            await _context.SaveChangesAsync();

            // Cache'i temizle
            _cacheService.Remove($"room_{roomCode}");
            _cacheService.Remove($"participants_{roomCode}");

            // SignalR ile odaya katıl
            await _hubContext.Clients.Group(roomCode).SendAsync("UserJoined", creator.UserName);

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
        public async Task<IActionResult> JoinRoom(string roomCode, string playerName, string joinMethod, string userName = null, string email = null, string password = null)
        {
            if (string.IsNullOrWhiteSpace(roomCode))
            {
                ViewBag.Error = "Oda kodu zorunludur.";
                ViewBag.RoomCode = roomCode;
                return View("Join");
            }

            var room = await _context.GameRooms.FirstOrDefaultAsync(r => r.RoomCode == roomCode && r.IsActive);
            if (room == null)
            {
                ViewBag.Error = "Böyle bir oda bulunamadı veya oda aktif değil.";
                ViewBag.RoomCode = roomCode;
                return View("Join");
            }

            User user;
            string displayName;

            if (joinMethod == "account")
            {
                // Hesap oluşturma yöntemi
                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    ViewBag.Error = "Hesap oluşturmak için tüm alanları doldurun.";
                    ViewBag.RoomCode = roomCode;
                    return View("Join");
                }

                // Kullanıcı adı kontrolü
                if (await _context.Users.AnyAsync(u => u.UserName == userName))
                {
                    ViewBag.Error = "Bu kullanıcı adı zaten alınmış.";
                    ViewBag.RoomCode = roomCode;
                    return View("Join");
                }

                // E-posta kontrolü
                if (await _context.Users.AnyAsync(u => u.Email == email))
                {
                    ViewBag.Error = "Bu e-posta zaten kayıtlı.";
                    ViewBag.RoomCode = roomCode;
                    return View("Join");
                }

                // Yeni kullanıcı oluştur
                user = new User
                {
                    UserName = userName,
                    Email = email,
                    PasswordHash = HashPassword(password),
                    CreatedAt = DateTime.UtcNow
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                displayName = userName;
            }
            else
            {
                // Misafir katılım yöntemi
                if (string.IsNullOrWhiteSpace(playerName))
                {
                    ViewBag.Error = "Misafir katılım için oyuncu adı gereklidir.";
                    ViewBag.RoomCode = roomCode;
                    return View("Join");
                }

                // Aynı isimle katılımı engelle
                var existing = await _context.RoomParticipants
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.GameRoomId == room.Id && p.User.UserName == playerName);
                
                if (existing != null)
                {
                    ViewBag.Error = "Bu isimle zaten bir oyuncu var.";
                    ViewBag.RoomCode = roomCode;
                    return View("Join");
                }

                // Benzersiz misafir email oluştur
                long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                string guestEmail = $"{playerName}_{timestamp}@guest.local";

                // Geçici kullanıcı oluştur
                user = new User
                {
                    UserName = playerName,
                    Email = guestEmail,
                    PasswordHash = "guest",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                displayName = playerName;
            }

            // Odaya katılımcı olarak ekle
            var participant = new RoomParticipant
            {
                UserId = user.Id,
                GameRoomId = room.Id
            };
            _context.RoomParticipants.Add(participant);
            await _context.SaveChangesAsync();

            // Cache'i temizle
            _cacheService.Remove($"room_{roomCode}");
            _cacheService.Remove($"participants_{roomCode}");

            // SignalR ile odaya katıl
            await _hubContext.Clients.Group(roomCode).SendAsync("UserJoined", displayName);

            // Hesap oluşturulduysa otomatik giriş yap
            if (joinMethod == "account")
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName)
                };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                
                TempData["LoginSuccess"] = $"Hoş geldiniz, {user.UserName}! Hesabınız oluşturuldu ve oyuna katıldınız.";
            }

            return RedirectToAction("Lobby", new { code = roomCode });
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Lobby(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return RedirectToAction("Join");
            
            // Cache'den oda bilgilerini al
            var cacheKey = $"room_{code}";
            GameRoom? room = _cacheService.Get<GameRoom>(cacheKey);
            
            if (room == null)
            {
                room = await _context.GameRooms.Include(r => r.Creator).FirstOrDefaultAsync(r => r.RoomCode == code);
                if (room == null)
                    return RedirectToAction("Join");
                
                // Cache'e kaydet (5 dakika)
                _cacheService.Set(cacheKey, room, TimeSpan.FromMinutes(5));
            }

            // Cache'den katılımcıları al
            var participantsCacheKey = $"participants_{code}";
            List<RoomParticipant>? participants = _cacheService.Get<List<RoomParticipant>>(participantsCacheKey);
            
            if (participants == null)
            {
                participants = await _context.RoomParticipants
                    .Where(p => p.GameRoomId == room.Id)
                    .Include(p => p.User)
                    .OrderBy(p => p.JoinedAt)
                    .ToListAsync();
                
                // Cache'e kaydet (2 dakika)
                _cacheService.Set(participantsCacheKey, participants, TimeSpan.FromMinutes(2));
            }

            // Odaya ait aktif oyun state'i
            var state = await _context.GameStates.FirstOrDefaultAsync(s => s.GameRoomId == room.Id && s.Started && !s.Finished);
            var votes = await _context.Votes.Where(v => v.GameRoomId == room.Id).ToListAsync();
            
            ViewBag.Room = room;
            ViewBag.Participants = participants;
            ViewBag.GameState = state;
            ViewBag.Votes = votes;
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
            if (selectedCategories == null || selectedCategories.Count == 0 || selectedWords == null || selectedWords.Count == 0)
            {
                TempData["SelectError"] = "En az bir kategori ve bir kelime seçmelisiniz.";
                return RedirectToAction("SelectWords", new { code });
            }
            if (selectedDurationMinutes == null || selectedDurationMinutes < 2 || selectedDurationMinutes > 15)
            {
                TempData["SelectError"] = "Oyun süresi 2-15 dakika arasında olmalı.";
                return RedirectToAction("SelectWords", new { code });
            }
            room.SelectedCategories = string.Join(",", selectedCategories);
            room.SelectedWords = string.Join(",", selectedWords);
            room.SelectedDuration = selectedDurationMinutes.Value * 60;
            _context.GameRooms.Update(room);
            await _context.SaveChangesAsync();

            // Cache'i temizle
            _cacheService.Remove($"room_{code}");

            // SignalR ile güncelleme gönder
            await _hubContext.Clients.Group(code).SendAsync("GameStateUpdated", new { 
                selectedCategories = selectedCategories, 
                selectedWords = selectedWords, 
                selectedDuration = selectedDurationMinutes 
            });

            return RedirectToAction("Lobby", new { code });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartGame(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return RedirectToAction("Lobby", new { code });
            var room = await _context.GameRooms.FirstOrDefaultAsync(r => r.RoomCode == code && r.IsActive);
            if (room == null)
                return RedirectToAction("Lobby", new { code });
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (room.CreatorId != userId)
                return RedirectToAction("Lobby", new { code });
            // Oyun zaten başlatılmış mı kontrol et
            var existingState = await _context.GameStates.FirstOrDefaultAsync(s => s.GameRoomId == room.Id && s.Started && !s.Finished);
            if (existingState != null)
                return RedirectToAction("Lobby", new { code });
            // Katılımcıları al
            var participants = await _context.RoomParticipants.Where(p => p.GameRoomId == room.Id).ToListAsync();
            if (participants.Count < 3)
            {
                TempData["GameError"] = "Oyun başlatmak için en az 3 oyuncu olmalı.";
                return RedirectToAction("Lobby", new { code });
            }
            // Rastgele casus seç
            var rnd = new Random();
            var spyParticipant = participants[rnd.Next(participants.Count)];
            var selectedWords = room.SelectedWords?.Split(',').ToList() ?? new List<string>();
            var selectedWord = selectedWords[rnd.Next(selectedWords.Count)];

            // Oyun state'i oluştur
            var gameState = new GameState
            {
                GameRoomId = room.Id,
                Started = true,
                Finished = false,
                SpyParticipantId = spyParticipant.Id,
                SelectedWord = selectedWord,
                StartedAt = DateTime.UtcNow,
                DurationSeconds = room.SelectedDuration ?? 480
            };
            _context.GameStates.Add(gameState);

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

            await _context.SaveChangesAsync();

            // Cache'i temizle
            _cacheService.Remove($"room_{code}");
            _cacheService.Remove($"participants_{code}");

            // SignalR ile oyun başlatma bildirimi
            await _hubContext.Clients.Group(code).SendAsync("GameStarted", code);

            return RedirectToAction("Lobby", new { code });
        }

        [HttpGet]
        public async Task<IActionResult> ShowRole(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return RedirectToAction("Lobby", new { code });
            var room = await _context.GameRooms.FirstOrDefaultAsync(r => r.RoomCode == code);
            if (room == null)
                return RedirectToAction("Lobby", new { code });
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var participant = await _context.RoomParticipants.FirstOrDefaultAsync(p => p.GameRoomId == room.Id && p.UserId == userId);
            if (participant == null)
                return RedirectToAction("Lobby", new { code });
            var role = await _context.PlayerRoles.FirstOrDefaultAsync(r => r.RoomParticipantId == participant.Id);
            if (role == null)
                return RedirectToAction("Lobby", new { code });
            ViewBag.Room = room;
            ViewBag.Role = role;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Vote(string code, int targetParticipantId)
        {
            if (string.IsNullOrWhiteSpace(code))
                return RedirectToAction("Lobby", new { code });
            var room = await _context.GameRooms.FirstOrDefaultAsync(r => r.RoomCode == code);
            if (room == null)
                return RedirectToAction("Lobby", new { code });
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var participant = await _context.RoomParticipants.FirstOrDefaultAsync(p => p.GameRoomId == room.Id && p.UserId == userId);
            if (participant == null)
                return RedirectToAction("Lobby", new { code });
            // Oyuncu zaten oy kullandı mı?
            var existingVote = await _context.Votes.FirstOrDefaultAsync(v => v.GameRoomId == room.Id && v.VoterParticipantId == participant.Id);
            if (existingVote != null)
            {
                TempData["VoteError"] = "Zaten oy kullandınız.";
                return RedirectToAction("Lobby", new { code });
            }
            // Hedef katılımcı geçerli mi?
            var target = await _context.RoomParticipants.FirstOrDefaultAsync(p => p.Id == targetParticipantId && p.GameRoomId == room.Id);
            if (target == null)
            {
                TempData["VoteError"] = "Geçersiz oyuncu seçimi.";
                return RedirectToAction("Lobby", new { code });
            }
            var vote = new Vote
            {
                GameRoomId = room.Id,
                VoterParticipantId = participant.Id,
                TargetParticipantId = target.Id
            };
            _context.Votes.Add(vote);
            await _context.SaveChangesAsync();

            // SignalR ile oy bildirimi
            await _hubContext.Clients.Group(code).SendAsync("VoteReceived", targetParticipantId, participant.User?.UserName ?? "Anonymous");

            // Oylama tamamlandı mı kontrol et
            var totalVotes = await _context.Votes.CountAsync(v => v.GameRoomId == room.Id);
            var totalParticipants = await _context.RoomParticipants.CountAsync(p => p.GameRoomId == room.Id);
            if (totalVotes >= totalParticipants)
            {
                // Oyun bitir
                return await FinishGameInternal(code, room.Id);
            }
            return RedirectToAction("Lobby", new { code });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinishByTimeout(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return RedirectToAction("Lobby", new { code });
            var room = await _context.GameRooms.FirstOrDefaultAsync(r => r.RoomCode == code);
            if (room == null)
                return RedirectToAction("Lobby", new { code });
            var state = await _context.GameStates.FirstOrDefaultAsync(s => s.GameRoomId == room.Id && s.Started && !s.Finished);
            if (state == null)
                return RedirectToAction("Lobby", new { code });
            // Süre doldu, casus kazandı
            state.Finished = true;
            state.FinishMessage = "Süre doldu! Casus kazandı.";
            _context.GameStates.Update(state);
            await _context.SaveChangesAsync();

            // Cache'i temizle
            _cacheService.Remove($"room_{code}");
            _cacheService.Remove($"participants_{code}");

            // SignalR ile oyun bitirme bildirimi
            await _hubContext.Clients.Group(code).SendAsync("GameEnded", "Süre doldu! Casus kazandı.");

            return RedirectToAction("Lobby", new { code });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rematch(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return RedirectToAction("Lobby", new { code });
            var room = await _context.GameRooms.FirstOrDefaultAsync(r => r.RoomCode == code);
            if (room == null)
                return RedirectToAction("Lobby", new { code });
            // Oda aktif değilse veya kelime seçilmemişse rematch başlatılamaz
            if (!room.IsActive || string.IsNullOrEmpty(room.SelectedWords))
                return RedirectToAction("Lobby", new { code });
            // Katılımcılar
            var participants = await _context.RoomParticipants.Where(p => p.GameRoomId == room.Id).ToListAsync();
            if (participants.Count < 3)
            {
                TempData["GameError"] = "Rematch için en az 3 oyuncu olmalı.";
                return RedirectToAction("Lobby", new { code });
            }
            // Eski oyun state'ini temizle
            var oldStates = await _context.GameStates.Where(s => s.GameRoomId == room.Id).ToListAsync();
            var oldRoles = await _context.PlayerRoles.Where(r => participants.Select(p => p.Id).Contains(r.RoomParticipantId)).ToListAsync();
            var oldVotes = await _context.Votes.Where(v => v.GameRoomId == room.Id).ToListAsync();
            _context.GameStates.RemoveRange(oldStates);
            _context.PlayerRoles.RemoveRange(oldRoles);
            _context.Votes.RemoveRange(oldVotes);
            await _context.SaveChangesAsync();

            // Cache'i temizle
            _cacheService.Remove($"room_{code}");
            _cacheService.Remove($"participants_{code}");

            // SignalR ile rematch bildirimi
            await _hubContext.Clients.Group(code).SendAsync("GameStateUpdated", new { rematch = true });

            return RedirectToAction("Lobby", new { code });
        }

        private async Task<IActionResult> FinishGameInternal(string code, int roomId)
        {
            var state = await _context.GameStates.FirstOrDefaultAsync(s => s.GameRoomId == roomId && s.Started && !s.Finished);
            if (state == null)
                return RedirectToAction("Lobby", new { code });
            var votes = await _context.Votes.Where(v => v.GameRoomId == roomId).ToListAsync();
            var participants = await _context.RoomParticipants.Where(p => p.GameRoomId == roomId).ToListAsync();
            var spyParticipant = participants.FirstOrDefault(p => p.Id == state.SpyParticipantId);
            var voteCounts = votes.GroupBy(v => v.TargetParticipantId).ToDictionary(g => g.Key, g => g.Count());
            var mostVotedParticipantId = voteCounts.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key;
            var mostVotedParticipant = participants.FirstOrDefault(p => p.Id == mostVotedParticipantId);
            string result;
            if (mostVotedParticipant?.Id == spyParticipant?.Id)
            {
                result = $"Oyuncular kazandı! Doğru tahmin: {spyParticipant?.User?.UserName} ajandı.";
            }
            else
            {
                result = $"Oyuncular kaybetti! Seçilen kişi ajan değildi. Ajan {spyParticipant?.User?.UserName} idi.";
            }
            state.Finished = true;
            state.FinishMessage = result;
            _context.GameStates.Update(state);
            await _context.SaveChangesAsync();

            // Cache'i temizle
            _cacheService.Remove($"room_{code}");
            _cacheService.Remove($"participants_{code}");

            // SignalR ile oyun sonucu bildirimi
            await _hubContext.Clients.Group(code).SendAsync("GameEnded", result);

            return RedirectToAction("Lobby", new { code });
        }

        private string GenerateRoomCode()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
} 