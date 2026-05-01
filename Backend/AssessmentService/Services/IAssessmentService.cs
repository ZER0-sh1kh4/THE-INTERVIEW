using AssessmentService.DTOs;
using AssessmentService.Models;

namespace AssessmentService.Services
{
    /// <summary>
    /// Defines assessment operations.
    /// </summary>
    public interface IAssessmentService
    {
        Task<StartAssessmentResponse?> StartAssessmentAsync(int userId, bool isPremium, StartAssessmentRequest request);
        Task<List<QuestionDto>> GetNextBatchAsync(int userId, int assessmentId, int currentCount, int batchSize = 5);
        Task<int> WarmUpCacheAsync(string domain, string difficulty, int targetCount = 3);
        Task<object?> SubmitAssessmentAsync(int userId, bool isPremium, SubmitAssessmentRequest request);
        Task<IEnumerable<AssessmentResult>> GetUserAssessmentsAsync(int userId);
        Task<object?> GetAssessmentResultAsync(int userId, int assessmentId, bool isPremium);
        Task<IEnumerable<AssessmentResult>> GetAllAssessmentsAsync();
        Task<IEnumerable<MCQQuestion>> GetAllQuestionsAsync();
        Task<MCQQuestion> AddQuestionAsync(CreateQuestionRequest request);
        Task<MCQQuestion?> UpdateQuestionAsync(int questionId, UpdateQuestionRequest request);
        Task<bool> DeleteQuestionAsync(int questionId);
    }
}

