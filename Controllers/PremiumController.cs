using casus_oyunu.Data;
using casus_oyunu.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Linq;

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
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            ViewBag.User = user;
            
            // Basit istatistikler
            var userStats = await _context.RoomParticipants
                .Where(p => p.UserId == userId)
                .Include(p => p.GameRoom)
                .ToListAsync();

            var totalGames = userStats.Count;
            ViewBag.TotalGames = totalGames;
            ViewBag.UserName = user.UserName;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Statistics()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            // Kullanıcının oyun istatistikleri
            var userStats = await _context.RoomParticipants
                .Where(p => p.UserId == userId)
                .Include(p => p.GameRoom)
                .ToListAsync();

            var totalGames = userStats.Count;
            var gamesWon = userStats.Count(p => p.GameRoom.GameSessions.Any(s => s.GameFinished && s.Winner == "players"));
            var gamesLost = totalGames - gamesWon;

            ViewBag.TotalGames = totalGames;
            ViewBag.GamesWon = gamesWon;
            ViewBag.GamesLost = gamesLost;
            ViewBag.WinRate = totalGames > 0 ? (double)gamesWon / totalGames * 100 : 0;

            return View();
        }
    }
} 