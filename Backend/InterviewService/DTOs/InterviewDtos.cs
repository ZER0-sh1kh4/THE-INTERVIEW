using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InterviewService.DTOs
{
    public class StartInterviewRequest
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Domain { get; set; } = string.Empty;
    }

    public class WarmUpRequest
    {
        [Required]
        public string Domain { get; set; } = string.Empty;
        public int TargetCount { get; set; } = 3;
    }
    
    public class QuestionResponseDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public string? OptionA { get; set; }
        public string? OptionB { get; set; }
        public string? OptionC { get; set; }
        public string? OptionD { get; set; }
        public int OrderIndex { get; set; }
    }

    public class SubmitInterviewRequest
    {
        [Range(1, int.MaxValue)]
        public int InterviewId { get; set; }
        [Required]
        [MinLength(1)]
        public List<InterviewAnswerSubmission> Answers { get; set; } = new List<InterviewAnswerSubmission>();
    }

    public class InterviewAnswerSubmission
    {
        [Range(1, int.MaxValue)]
        public int QuestionId { get; set; }
        [Required]
        public string AnswerText { get; set; } = string.Empty;
    }
    
    public class GeminiQuestionRootDto
    {
        public List<GeminiQuestionDto> Questions { get; set; } = new List<GeminiQuestionDto>();
    }

    public class GeminiQuestionDto
    {
        public int id { get; set; }
        public string question { get; set; } = string.Empty;
        public string type { get; set; } = string.Empty;
        public string difficulty { get; set; } = string.Empty;
        
        // Legacy/fallback fields
        public string text { get; set; } = string.Empty;
        public string questionType { get; set; } = string.Empty;
        public string? optionA { get; set; }
        public string? optionB { get; set; }
        public string? optionC { get; set; }
        public string? optionD { get; set; }
        public string? correctAnswer { get; set; }
        public string? idealAnswer { get; set; }
        public string? evaluationCriteria { get; set; }
        public string subtopic { get; set; } = string.Empty;
    }
}
