using Microsoft.EntityFrameworkCore;
using CinemaManager.API.Models;

namespace CinemaManager.API.Data
{
    public class CinemaDbContext : DbContext
    {
        public CinemaDbContext(DbContextOptions<CinemaDbContext> options) : base(options) { }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<Hall> Halls { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Ticket> Tickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Ticket>(b =>
            {
                b.HasOne(t => t.Session)
                    .WithMany()
                    .HasForeignKey(t => t.SessionId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Prevent double-selling same seat for same session.
                b.HasIndex(t => new { t.SessionId, t.Row, t.Seat })
                    .IsUnique();
            });
        }
    }
}