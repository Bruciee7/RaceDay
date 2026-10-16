
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data
{
    public class RaceDayContext : DbContext
    {
        public RaceDayContext(DbContextOptions<RaceDayContext> options)
            : base(options)
        {
        }

        public DbSet<RaceDayUser> Users => Set<RaceDayUser>();
        public DbSet<RaceDayEvent> Events => Set<RaceDayEvent>();
        public DbSet<RaceDayCategory> Categories => Set<RaceDayCategory>();
        public DbSet<RaceDayEnrolment> Enrolments => Set<RaceDayEnrolment>();
        public DbSet<RaceDayResult> Results => Set<RaceDayResult>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RaceDayUser>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<RaceDayUser>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<RaceDayEvent>()
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<RaceDayEnrolment>()
                .Property(e => e.Status)
                .HasDefaultValue("Pending");

            modelBuilder.Entity<RaceDayEnrolment>()
                .Property(e => e.EnrolmentDate)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<RaceDayEnrolment>()
                .HasIndex(e => new { e.ParticipantId, e.EventId })
                .IsUnique();

            modelBuilder.Entity<RaceDayResult>()
                .HasIndex(r => r.EnrolmentId)
                .IsUnique();

            modelBuilder.Entity<RaceDayEvent>()
                .HasOne(e => e.Organiser)
                .WithMany(u => u.Events)
                .HasForeignKey(e => e.OrganiserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<RaceDayEnrolment>()
                .HasOne(e => e.Participant)
                .WithMany(u => u.Enrolments)
                .HasForeignKey(e => e.ParticipantId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<RaceDayEnrolment>()
                .HasOne(e => e.Event)
                .WithMany(e => e.Enrolments)
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<RaceDayEnrolment>()
                .HasOne(e => e.Category)
                .WithMany(c => c.Enrolments)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<RaceDayEnrolment>()
                .HasOne(e => e.Result)
                .WithOne(r => r.Enrolment)
                .HasForeignKey<RaceDayResult>(r => r.EnrolmentId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
