using Microsoft.EntityFrameworkCore;
using KMCEventAPI.Models;

namespace KMCEventAPI.Data
{
    public class KMCEventDbContext : DbContext
    {
        public KMCEventDbContext(
            DbContextOptions<KMCEventDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<Registration> Registrations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================
            // TABLE NAMES
            // =========================

            modelBuilder.Entity<User>()
                .ToTable("Users");

            modelBuilder.Entity<Event>()
                .ToTable("Events");

            modelBuilder.Entity<Participant>()
                .ToTable("Participants");

            modelBuilder.Entity<Registration>()
                .ToTable("Registrations");


            // =========================
            // USER
            // =========================

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Name)
                    .IsRequired();

                entity.Property(u => u.Email)
                    .IsRequired();

                entity.Property(u => u.Password)
                    .IsRequired();

                entity.Property(u => u.Role)
                    .IsRequired();
            });


            // =========================
            // EVENT
            // =========================

            modelBuilder.Entity<Event>(entity =>
            {
                entity.Property(e => e.Title)
                    .IsRequired();

                entity.Property(e => e.Description)
                    .IsRequired();

                entity.Property(e => e.Type)
                    .IsRequired();

                entity.Property(e => e.Time)
                    .IsRequired();

                entity.Property(e => e.Location)
                    .IsRequired();

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.OrganizerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            // =========================
            // PARTICIPANT
            // =========================

            modelBuilder.Entity<Participant>(entity =>
            {
                entity.Property(p => p.Name)
                    .IsRequired();

                entity.Property(p => p.Email)
                    .IsRequired();

                entity.Property(p => p.Phone)
                    .IsRequired();
            });


            // =========================
            // REGISTRATION
            // =========================

            modelBuilder.Entity<Registration>(entity =>
            {
                // Registration -> Event
                entity.HasOne(r => r.Event)
                    .WithMany()
                    .HasForeignKey(r => r.EventId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Registration -> Participant
                entity.HasOne(r => r.Participant)
                    .WithMany()
                    .HasForeignKey(r => r.ParticipantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}