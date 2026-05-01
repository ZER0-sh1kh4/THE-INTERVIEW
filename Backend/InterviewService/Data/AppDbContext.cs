using InterviewService.Models;
using Microsoft.EntityFrameworkCore;

namespace InterviewService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Interview> Interviews { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<InterviewAnswer> InterviewAnswers { get; set; }
        public DbSet<InterviewResult> InterviewResults { get; set; }
        public DbSet<GlobalInterviewQuestion> GlobalInterviewQuestions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
