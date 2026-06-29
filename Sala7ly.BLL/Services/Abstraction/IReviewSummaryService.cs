namespace Sala7ly.BLL.Services.Abstraction
{
    // Sala7ly.BLL/AI/Interfaces/IReviewSummaryService.cs
    public interface IReviewSummaryService
    {
        Task SummarizeAllAsync();
        Task SummarizeTechnicianAsync(int technicianId);
    }
}