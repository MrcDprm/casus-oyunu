using Microsoft.EntityFrameworkCore;

namespace casus_oyunu.Models
{
    public class GameDbContext : DbContext
    {
        public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }

        public DbSet<GameSession> GameSessions { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Category> Categories { get; set; }
    }
} 