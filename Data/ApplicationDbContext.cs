using Microsoft.EntityFrameworkCore;
using casus_oyunu.Models;

namespace casus_oyunu.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<GameRoom> GameRooms { get; set; }
        public DbSet<RoomParticipant> RoomParticipants { get; set; }
        public DbSet<GameState> GameStates { get; set; }
        public DbSet<PlayerRole> PlayerRoles { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<GameSession> GameSessions { get; set; }
        public DbSet<PlayerPosition> PlayerPositions { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Vote ilişkileri
            modelBuilder.Entity<Vote>()
                .HasOne(v => v.Voter)
                .WithMany()
                .HasForeignKey(v => v.VoterParticipantId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Vote>()
                .HasOne(v => v.Target)
                .WithMany()
                .HasForeignKey(v => v.TargetParticipantId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // RoomParticipant ilişkileri
            modelBuilder.Entity<RoomParticipant>()
                .HasOne(rp => rp.User)
                .WithMany()
                .HasForeignKey(rp => rp.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<RoomParticipant>()
                .HasOne(rp => rp.GameRoom)
                .WithMany()
                .HasForeignKey(rp => rp.GameRoomId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // PlayerPosition ilişkisi
            modelBuilder.Entity<PlayerPosition>()
                .HasOne(pp => pp.RoomParticipant)
                .WithMany()
                .HasForeignKey(pp => pp.RoomParticipantId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // ChatMessage ilişkisi
            modelBuilder.Entity<ChatMessage>()
                .HasOne(cm => cm.Sender)
                .WithMany()
                .HasForeignKey(cm => cm.SenderParticipantId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // GameSession ilişkisi
            modelBuilder.Entity<GameSession>()
                .HasOne(gs => gs.GameRoom)
                .WithMany()
                .HasForeignKey(gs => gs.GameRoomId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
} 