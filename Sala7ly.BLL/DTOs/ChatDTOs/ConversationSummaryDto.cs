namespace Sala7ly.BLL.DTOs.ChatDTOs
{
    public record ConversationSummaryDto(
    int RequestId,
    string OtherPartyName,
    string? OtherPartyImage,
    string? LastMessage,
    DateTime? LastMessageTime,
    int UnreadCount
);
}
