using AssessmentService.Models;
using Microsoft.EntityFrameworkCore;

namespace AssessmentService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<MCQQuestion> MCQQuestions { get; set; }
        public DbSet<Assessment> Assessments { get; set; }
        public DbSet<UserAnswer> UserAnswers { get; set; }
        public DbSet<AssessmentResult> AssessmentResults { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
