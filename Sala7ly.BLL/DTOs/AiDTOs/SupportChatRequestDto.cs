namespace Sala7ly.BLL.DTOs.AiDTOs
{
    public class SupportChatRequestDto
    {
        public string Message { get; set; }
        public string? SessionId { get; set; }
        public List<ChatTurn> History { get; set; } = new();
    }
}
