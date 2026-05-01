using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AssessmentService.DTOs
{
    public class StartAssessmentRequest
    {
        [Required]
        public string Domain { get; set; } = string.Empty;

        // FRESHLY ADDED: frontend can request exam size without changing database schema.
        [Range(1, 60)]
        public int QuestionCount { get; set; } = 10;

        // Optional difficulty helps Gemini tune generated assessment questions.
        public string Difficulty { get; set; } = "Medium";
    }

    public class StartAssessmentResponse
    {
        public int AssessmentId { get; set; }
        public int TimeLimitMinutes { get; set; }
        public System.DateTime ExpiresAt { get; set; }
        public List<QuestionDto> Questions { get; set; } = new List<QuestionDto>();
        public int TotalExpected { get; set; }
        public bool HasMore { get; set; }
    }

    public class QuestionDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string OptionC { get; set; } = string.Empty;
        public string OptionD { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
    }

    public class SubmitAssessmentRequest
    {
        [Range(1, int.MaxValue)]
        public int AssessmentId { get; set; }
        [Required]
        [MinLength(1)]
        public List<AnswerSubmission> Answers { get; set; } = new List<AnswerSubmission>();
        public int? TotalExpected { get; set; }
    }

    public class AnswerSubmission
    {
        [Range(1, int.MaxValue)]
        public int QuestionId { get; set; }
        [Required]
        [RegularExpression("^[A-Da-d]$", ErrorMessage = "Selected option must be A, B, C, or D.")]
        public string SelectedOption { get; set; } = string.Empty;
    }

    public class CreateQuestionRequest
    {
        [Required]
        public string Domain { get; set; } = string.Empty;
        [Required]
        public string Text { get; set; } = string.Empty;
        [Required]
        public string OptionA { get; set; } = string.Empty;
        [Required]
        public string OptionB { get; set; } = string.Empty;
        [Required]
        public string OptionC { get; set; } = string.Empty;
        [Required]
        public string OptionD { get; set; } = string.Empty;
        [Required]
        [RegularExpression("^[A-Da-d]$", ErrorMessage = "Correct option must be one of A, B, C, or D.")]
        public string CorrectOption { get; set; } = string.Empty;
        [Required]
        public string Subtopic { get; set; } = string.Empty;
    }

    public class UpdateQuestionRequest
    {
        [Required]
        public string Domain { get; set; } = string.Empty;
        [Required]
        public string Text { get; set; } = string.Empty;
        [Required]
        public string OptionA { get; set; } = string.Empty;
        [Required]
        public string OptionB { get; set; } = string.Empty;
        [Required]
        public string OptionC { get; set; } = string.Empty;
        [Required]
        public string OptionD { get; set; } = string.Empty;
        [Required]
        [RegularExpression("^[A-Da-d]$", ErrorMessage = "Correct option must be one of A, B, C, or D.")]
        public string CorrectOption { get; set; } = string.Empty;
        [Required]
        public string Subtopic { get; set; } = string.Empty;
    }

    public class WarmUpRequest
    {
        [Required]
        public string Domain { get; set; } = string.Empty;
        public string Difficulty { get; set; } = "Medium";
        public int TargetCount { get; set; } = 3;
    }
}
