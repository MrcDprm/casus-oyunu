using Microsoft.AspNetCore.Mvc;
using casus_oyunu.Models;
using casus_oyunu.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;

namespace casus_oyunu.Controllers
{
    [Authorize]
    public class GameController : Controller
    {
        private readonly ApplicationDbContext _context;
        
        public GameController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.Categories = CategoryData.Categories;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddPlayer(string playerName, string categoryNames, string wordNames, int gameDuration)
        {
            if (!string.IsNullOrWhiteSpace(playerName))
            {
                // Oyuncu ekleme işlemi artık veritabanı üzerinden yapılacak
                // Bu kısım RoomController'da hallediliyor
            }
            
            var selectedCategories = !string.IsNullOrEmpty(categoryNames) ? categoryNames.Split(',').ToList() : new List<string>();
            var selectedWords = !string.IsNullOrEmpty(wordNames) ? wordNames.Split(',').ToList() : new List<string>();
            int duration = (gameDuration >= 10 && gameDuration <= 720) ? gameDuration : 480;
            
            ViewBag.Categories = CategoryData.Categories;
            ViewBag.GameDuration = duration;
            ViewBag.SelectedCategories = selectedCategories;
            ViewBag.SelectedWords = selectedWords;
            return View("Index");
        }

        [HttpPost]
        public async Task<IActionResult> SetGameOptions(string categoryNames, string wordNames, int gameDuration)
        {
            var selectedCategories = !string.IsNullOrEmpty(categoryNames) ? categoryNames.Split(',').ToList() : new List<string>();
            var selectedWords = !string.IsNullOrEmpty(wordNames) ? wordNames.Split(',').ToList() : new List<string>();
            
            int duration = gameDuration;
            if (duration < 10) duration = 10;
            else if (duration > 720) duration = 720;
            
            ViewBag.Categories = CategoryData.Categories;
            ViewBag.GameDuration = duration;
            ViewBag.SelectedCategories = selectedCategories;
            ViewBag.SelectedWords = selectedWords;
            return View("Index");
        }

        [HttpPost]
        public async Task<IActionResult> StartGame()
        {
            // Bu action artık RoomController'da hallediliyor
            // GameController sadece eski oyun modu için kullanılacak
            return RedirectToAction("Index", "Room");
        }

        [HttpGet]
        public IActionResult ShowCards()
        {
            // Bu view artık RoomController'da hallediliyor
            return RedirectToAction("Index", "Room");
        }

        [HttpPost]
        public async Task<IActionResult> ResetGame()
        {
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemovePlayer(int playerIndex)
        {
            ViewBag.Categories = CategoryData.Categories;
            return View("Index");
        }

        [HttpPost]
        public async Task<IActionResult> FinishGame(string selectedPlayer)
        {
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DrawGame()
        {
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Vote(string voter, string voted)
        {
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult ShowVotes()
        {
            return RedirectToAction("Index");
        }
    }
} 