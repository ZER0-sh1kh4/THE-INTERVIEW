using System;

namespace InterviewService.Models
{
    public class Interview
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public string Type { get; set; } = "Normal"; // Normal/Premium
        public string Status { get; set; } = "Pending"; // Pending/InProgress/Completed
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public class Question
    {
        public int Id { get; set; }
        public int InterviewId { get; set; }
        public string Text { get; set; } = string.Empty;
        public string QuestionType { get; set; } = "Subjective";
        public string? OptionA { get; set; }
        public string? OptionB { get; set; }
        public string? OptionC { get; set; }
        public string? OptionD { get; set; }
        public string? CorrectAnswer { get; set; }
        public string Subtopic { get; set; } = string.Empty;
        public string Source { get; set; } = "AI";
        public int OrderIndex { get; set; }
    }

    public class GlobalInterviewQuestion
    {
        public int Id { get; set; }
        public string Domain { get; set; } = string.Empty;
        public string Difficulty { get; set; } = "Medium";
        public string Text { get; set; } = string.Empty;
        public string QuestionType { get; set; } = "Subjective";
        public string? IdealAnswer { get; set; }
        public string Subtopic { get; set; } = string.Empty;
        public string Source { get; set; } = "AI";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class InterviewAnswer
    {
        public int Id { get; set; }
        public int InterviewId { get; set; }
        public int QuestionId { get; set; }
        public int UserId { get; set; }
        public string AnswerText { get; set; } = string.Empty;
        public bool? IsCorrect { get; set; }
        public int Score { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }

    public class InterviewResult
    {
        public int Id { get; set; }
        public int InterviewId { get; set; }
        public int UserId { get; set; }
        public int TotalScore { get; set; }
        public int MaxScore { get; set; }
        public double Percentage { get; set; }
        public string Grade { get; set; } = string.Empty;
        public string Feedback { get; set; } = string.Empty;
        public bool IsPremiumResult { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
