using Sala7ly.DAL.Enums;

namespace Sala7ly.BLL.DTOs.ChatDTOs
{
    public record ChatMessageResponseDto(
    int Id,
    int RequestId,
    string SenderId,
    string SenderName,
    string? Content,
    string[]? AttachmentUrls,
    ChatMessageType MessageType,
    int? DurationSeconds,
    bool IsRead,
    DateTime? ReadAt,
    DateTime SentAt
);
}
