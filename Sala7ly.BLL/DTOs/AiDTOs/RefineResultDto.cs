namespace Sala7ly.BLL.DTOs.AiDTOs
{
    public class RefineResultDto
    {
        public string RefinedDescription { get; set; }
        public string AiSummary { get; set; }
        public int? SuggestedCategoryId { get; set; }
        public string? SuggestedCategoryName { get; set; }
        public string SuggestedUrgency { get; set; }  // "low" | "medium" | "high"
        public string RefinementJson { get; set; }    // raw Q&A to store
    }
}
