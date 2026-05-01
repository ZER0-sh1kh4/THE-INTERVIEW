using InterviewService.DTOs;
using InterviewService.Models;

namespace InterviewService.Services
{
    /// <summary>
    /// Defines interview lifecycle operations.
    /// </summary>
    public interface IInterviewSvc
    {
        Task<Interview> StartInterviewAsync(int userId, bool isPremium, StartInterviewRequest request);
        Task<object> BeginInterviewAsync(int userId, bool isPremium, int interviewId);
        Task<int> WarmUpCacheAsync(string domain, int targetCount = 3);
        Task<object?> SubmitInterviewAsync(int userId, bool isPremium, SubmitInterviewRequest request);
        Task<IEnumerable<Interview>> GetMyInterviewsAsync(int userId);
        Task<object?> GetInterviewByIdAsync(int userId, int interviewId);
        Task<object?> GetResultAsync(int userId, bool isPremium, int interviewId);
        Task<IEnumerable<Interview>> GetAllInterviewsAsync();
    }
}
