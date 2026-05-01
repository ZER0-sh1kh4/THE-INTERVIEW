using Microsoft.EntityFrameworkCore;
using SubscriptionService.Models;

namespace SubscriptionService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<PaymentRecord> PaymentRecords { get; set; }
        public DbSet<WebhookEventLog> WebhookEventLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Subscription>()
                .Property(x => x.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PaymentRecord>()
                .Property(x => x.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<WebhookEventLog>()
                .HasIndex(x => x.EventId)
                .IsUnique();
        }
    }
}
