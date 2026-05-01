using IdentityService.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace IdentityService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    FullName = "Shikha Gangwar",
                    Email = "sh1kh4g@gmail.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Shikha@123"),
                    Role = "Admin",
                    IsPremium = true,
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new User
                {
                    Id = 2,
                    FullName = "Cyber Shuu",
                    Email = "Cybershuu639@gmail.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Cybershuu@123"),
                    Role = "Candidate",
                    IsPremium = false,
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}
