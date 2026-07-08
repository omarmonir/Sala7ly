namespace Sala7ly.BLL.DTOs.AiSupportDTOs
{
    public class AiSupportRequestDto
    {
        public string Message { get; set; } = string.Empty;
        // optional: prior turns for context (role = "user"/"assistant", content)
        public List<AiChatTurn>? History { get; set; }
    }

    public class AiChatTurn
    {
        public string Role { get; set; } = "user";
        public string Content { get; set; } = string.Empty;
    }

    public class AiSupportResponseDto
    {
        public string Reply { get; set; } = string.Empty;
    }
}