using casus_oyunu.Data;
using casus_oyunu.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace casus_oyunu.Controllers
{
    [Authorize]
    public class PremiumController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PremiumController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Users
                .Include(u => u.UserSubscriptions)
                .FirstOrDefaultAsync(u => u.Id == userId);

            var activeSubscription = user?.UserSubscriptions?
                .FirstOrDefault(s => s.IsActive);

            ViewBag.User = user;
            ViewBag.ActiveSubscription = activeSubscription;
            ViewBag.SubscriptionTypes = new[]
            {
                new { Type = SubscriptionType.Basic, Name = "Temel", Price = 19.99m, Features = new[] { "Reklamsız oyun", "Özel kategoriler", "İstatistikler" } },
                new { Type = SubscriptionType.Premium, Name = "Premium", Price = 39.99m, Features = new[] { "Temel özellikler", "Turnuva katılımı", "Özel temalar", "Öncelikli destek" } },
                new { Type = SubscriptionType.Pro, Name = "Pro", Price = 79.99m, Features = new[] { "Premium özellikler", "Turnuva oluşturma", "API erişimi", "Özel etkinlikler" } }
            };

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Subscribe(SubscriptionType subscriptionType)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            // Mevcut aktif aboneliği kontrol et
            var existingSubscription = await _context.UserSubscriptions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);

            if (existingSubscription != null)
            {
                TempData["Error"] = "Zaten aktif bir aboneliğiniz var.";
                return RedirectToAction("Index");
            }

            // Yeni abonelik oluştur
            var subscription = new UserSubscription
            {
                UserId = userId,
                Type = subscriptionType,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1), // 1 aylık abonelik
                PaymentMethod = "Credit Card", // Gerçek uygulamada ödeme sistemi entegrasyonu
                TransactionId = Guid.NewGuid().ToString(),
                Amount = GetSubscriptionPrice(subscriptionType),
                Currency = "TRY"
            };

            _context.UserSubscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"{subscriptionType} aboneliği başarıyla aktifleştirildi!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelSubscription()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            var activeSubscription = await _context.UserSubscriptions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);

            if (activeSubscription == null)
            {
                TempData["Error"] = "Aktif aboneliğiniz bulunmuyor.";
                return RedirectToAction("Index");
            }

            // Aboneliği iptal et (mevcut dönem sonuna kadar aktif kalır)
            activeSubscription.UpdatedAt = DateTime.UtcNow;
            _context.UserSubscriptions.Update(activeSubscription);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Aboneliğiniz iptal edildi. Mevcut dönem sonuna kadar kullanmaya devam edebilirsiniz.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Statistics()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            // Kullanıcının oyun istatistikleri
            var userStats = await _context.RoomParticipants
                .Where(p => p.UserId == userId)
                .Include(p => p.GameRoom)
                .ThenInclude(r => r.GameStates)
                .ToListAsync();

            var totalGames = userStats.Count;
            var gamesWon = userStats.Count(p => p.GameRoom.GameStates.Any(s => s.Finished && s.FinishMessage.Contains("kazandı")));
            var gamesLost = totalGames - gamesWon;

            ViewBag.TotalGames = totalGames;
            ViewBag.GamesWon = gamesWon;
            ViewBag.GamesLost = gamesLost;
            ViewBag.WinRate = totalGames > 0 ? (double)gamesWon / totalGames * 100 : 0;

            return View();
        }

        private decimal GetSubscriptionPrice(SubscriptionType type)
        {
            return type switch
            {
                SubscriptionType.Basic => 19.99m,
                SubscriptionType.Premium => 39.99m,
                SubscriptionType.Pro => 79.99m,
                _ => 0m
            };
        }
    }
} 