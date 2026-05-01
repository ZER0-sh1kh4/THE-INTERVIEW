using System;

namespace AssessmentService.Models
{
    public class MCQQuestion
    {
        public int Id { get; set; }
        public string Domain { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string OptionC { get; set; } = string.Empty;
        public string OptionD { get; set; } = string.Empty;
        public string CorrectOption { get; set; } = string.Empty; // A/B/C/D
        public string Subtopic { get; set; } = string.Empty;
        public int Marks { get; set; } = 1;
        public int OrderIndex { get; set; }
    }

    public class Assessment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Domain { get; set; } = string.Empty;
        public string Status { get; set; } = "NotStarted"; // NotStarted/InProgress/Completed/Expired
        public int AttemptNumber { get; set; }
        public int TimeLimitMinutes { get; set; } = 10;
        public DateTime StartedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class UserAnswer
    {
        public int Id { get; set; }
        public int AssessmentId { get; set; }
        public int QuestionId { get; set; }
        public int UserId { get; set; }
        public string SelectedOption { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }

    public class AssessmentResult
    {
        public int Id { get; set; }
        public int AssessmentId { get; set; }
        public int UserId { get; set; }
        public string Domain { get; set; } = string.Empty;
        public int Score { get; set; }
        public int MaxScore { get; set; }
        public double Percentage { get; set; }
        public string Grade { get; set; } = string.Empty;
        public bool IsPremiumResult { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
