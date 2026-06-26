namespace Sala7ly.BLL.DTOs.AiDTOs
{
    public class RefineRequestWithAnswersDto : RefineRequestDto
    {
        public List<string> AllAnswers { get; set; } = new();
    }
}
