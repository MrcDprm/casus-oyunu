using Microsoft.EntityFrameworkCore;
using casus_oyunu.Models;

namespace casus_oyunu.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<GameRoom> GameRooms { get; set; }
        public DbSet<RoomParticipant> RoomParticipants { get; set; }
        public DbSet<GameState> GameStates { get; set; }
        public DbSet<PlayerRole> PlayerRoles { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<UserSubscription> UserSubscriptions { get; set; }
        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<TournamentParticipant> TournamentParticipants { get; set; }
        public DbSet<TournamentMatch> TournamentMatches { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User - GameRoom relationship
            modelBuilder.Entity<GameRoom>()
                .HasOne(r => r.Creator)
                .WithMany(u => u.CreatedRooms)
                .HasForeignKey(r => r.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            // User - RoomParticipant relationship
            modelBuilder.Entity<RoomParticipant>()
                .HasOne(p => p.User)
                .WithMany(u => u.RoomParticipants)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // GameRoom - RoomParticipant relationship
            modelBuilder.Entity<RoomParticipant>()
                .HasOne(p => p.GameRoom)
                .WithMany(r => r.RoomParticipants)
                .HasForeignKey(p => p.GameRoomId)
                .OnDelete(DeleteBehavior.Restrict);

            // GameRoom - GameState relationship
            modelBuilder.Entity<GameState>()
                .HasOne(s => s.GameRoom)
                .WithMany(r => r.GameStates)
                .HasForeignKey(s => s.GameRoomId)
                .OnDelete(DeleteBehavior.Cascade);

            // RoomParticipant - PlayerRole relationship
            modelBuilder.Entity<PlayerRole>()
                .HasOne(r => r.RoomParticipant)
                .WithMany()
                .HasForeignKey(r => r.RoomParticipantId)
                .OnDelete(DeleteBehavior.Cascade);

            // GameRoom - Vote relationship
            modelBuilder.Entity<Vote>()
                .HasOne(v => v.GameRoom)
                .WithMany(r => r.Votes)
                .HasForeignKey(v => v.GameRoomId)
                .OnDelete(DeleteBehavior.Cascade);

            // RoomParticipant - Vote relationships
            modelBuilder.Entity<Vote>()
                .HasOne(v => v.VoterParticipant)
                .WithMany()
                .HasForeignKey(v => v.VoterParticipantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vote>()
                .HasOne(v => v.TargetParticipant)
                .WithMany()
                .HasForeignKey(v => v.TargetParticipantId)
                .OnDelete(DeleteBehavior.Restrict);

            // User - UserSubscription relationship
            modelBuilder.Entity<UserSubscription>()
                .HasOne(s => s.User)
                .WithMany(u => u.UserSubscriptions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User - Tournament relationship
            modelBuilder.Entity<Tournament>()
                .HasOne(t => t.Creator)
                .WithMany()
                .HasForeignKey(t => t.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Tournament - TournamentParticipant relationship
            modelBuilder.Entity<TournamentParticipant>()
                .HasOne(p => p.Tournament)
                .WithMany()
                .HasForeignKey(p => p.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);

            // User - TournamentParticipant relationship
            modelBuilder.Entity<TournamentParticipant>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Tournament - TournamentMatch relationship
            modelBuilder.Entity<TournamentMatch>()
                .HasOne(m => m.Tournament)
                .WithMany()
                .HasForeignKey(m => m.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);

            // GameRoom - TournamentMatch relationship
            modelBuilder.Entity<TournamentMatch>()
                .HasOne(m => m.GameRoom)
                .WithMany()
                .HasForeignKey(m => m.GameRoomId)
                .OnDelete(DeleteBehavior.SetNull);

            // User - TournamentMatch relationships
            modelBuilder.Entity<TournamentMatch>()
                .HasOne(m => m.Player1)
                .WithMany()
                .HasForeignKey(m => m.Player1Id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TournamentMatch>()
                .HasOne(m => m.Player2)
                .WithMany()
                .HasForeignKey(m => m.Player2Id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TournamentMatch>()
                .HasOne(m => m.Winner)
                .WithMany()
                .HasForeignKey(m => m.WinnerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes for better performance
            modelBuilder.Entity<GameRoom>()
                .HasIndex(r => r.RoomCode)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.UserName)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Tournament>()
                .HasIndex(t => t.Status);

            modelBuilder.Entity<Tournament>()
                .HasIndex(t => t.StartDate);

            modelBuilder.Entity<UserSubscription>()
                .HasIndex(s => s.UserId);

            modelBuilder.Entity<UserSubscription>()
                .HasIndex(s => s.Type);
        }
    }
} 