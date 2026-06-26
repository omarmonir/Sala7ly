namespace Sala7ly.BLL.DTOs.AiDTOs
{
    public class FollowUpRequestDto
    {
        public string RawDescription { get; set; }
        public List<string> PreviousAnswers { get; set; } = new();
        public List<string> Categories { get; set; } = new();
    }
}
