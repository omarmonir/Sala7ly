using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Sala7ly.API.Hubs;
using Sala7ly.BLL.DTOs.ChatDTOs;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Enums;
using Sala7ly.DAL.Repositories.Abstraction;


namespace Sala7ly.BLL.Services.Implementation
{
    public class ChatService : IChatService
        {
            private readonly IChatMessageRepository _chatRepo;
            private readonly IServiceRequestRepository _requestRepo;
            private readonly ITechnicianProfileRepository _techRepo;
            private readonly IFileService _files;
            private readonly IHubContext<ChatHub> _hubContext;

            public ChatService(
                IChatMessageRepository chatRepo,
                IServiceRequestRepository requestRepo,
                ITechnicianProfileRepository techRepo,
                IFileService files,
                IHubContext<ChatHub> hubContext)
            {
                _chatRepo = chatRepo;
                _requestRepo = requestRepo;
                _techRepo = techRepo;
                _files = files;
                _hubContext = hubContext;
            }

            public async Task<bool> CanAccessRequestAsync(string userId, int requestId)
            {
                var request = await _requestRepo.GetByIdAsync(requestId);
                if (request is null) return false;


                if (request.Profile?.UserId == userId) return true;

                var techProfile = await _techRepo.GetByUserIdAsync(userId);
                if (techProfile is null) return false;

                return request.SelectedBid?.TechnicianId == techProfile.Id;
            }

            public async Task<ChatMessageResponseDto> SaveMessageAsync(
                string senderId, SendMessageDto dto, IFormFileCollection? files)
            {
                if (!await CanAccessRequestAsync(senderId, dto.RequestId))
                    throw new UnauthorizedAccessException("ليس لديك صلاحية للوصول إلى هذه المحادثة.");

                ChatMessage message;

                if (dto.MessageType == ChatMessageType.text)
                {
                    if (string.IsNullOrWhiteSpace(dto.Content))
                        throw new InvalidOperationException("محتوى الرسالة لا يمكن أن يكون فارغاً.");

                    message = ChatMessage.CreateText(dto.RequestId, senderId, dto.Content);
                }
                else
                {
                    if (files is null || files.Count == 0)
                        throw new InvalidOperationException("يجب إرفاق ملف.");

                    var folder = dto.MessageType == ChatMessageType.voice_note
                        ? "chat/voice" : "chat/images";

                    var urls = new List<string>();
                    foreach (var file in files)
                        urls.Add(await _files.SaveFileAsync(file, folder));

                    message = ChatMessage.CreateWithAttachments(
                        dto.RequestId, senderId,
                        urls.ToArray(),
                        dto.MessageType,
                        dto.DurationSeconds);
                }

                await _chatRepo.AddAsync(message);
                await _chatRepo.SaveChangesAsync();

                var saved = await _chatRepo.GetByIdWithSenderAsync(message.Id);
                var response = MapToDto(saved!);

                // Broadcast to SignalR group
                await _hubContext.Clients
                    .Group($"request_{dto.RequestId}")
                    .SendAsync("ReceiveMessage", response);

                return response;
            }

            public async Task<List<ChatMessageResponseDto>> GetHistoryAsync(
                string userId, int requestId, int page = 1, int pageSize = 30)
            {
                if (!await CanAccessRequestAsync(userId, requestId))
                    throw new UnauthorizedAccessException("ليس لديك صلاحية للوصول إلى هذه المحادثة.");

                var messages = await _chatRepo.GetByRequestIdAsync(requestId, page, pageSize);
                return messages.Select(MapToDto).ToList();
            }

            public async Task MarkAsReadAsync(int requestId, string userId)
            {
                await _chatRepo.MarkAllAsReadAsync(requestId, userId);
                await _chatRepo.SaveChangesAsync();
            }

            public async Task<List<ConversationSummaryDto>> GetConversationsAsync(string userId)
            {
                var requestIds = await _chatRepo.GetRequestIdsForUserAsync(userId);
                var result = new List<ConversationSummaryDto>();

                foreach (var requestId in requestIds)
                {
                    var lastMessage = await _chatRepo.GetLastMessageAsync(requestId);
                    var unread = await _chatRepo.GetUnreadCountAsync(requestId, userId);
                    var request = await _requestRepo.GetByIdWithPartiesAsync(requestId);

                    if (request is null) continue;

                    var isCustomer = request.Profile?.UserId == userId;
                    var otherUser = isCustomer
                        ? request.SelectedBid?.Technician?.User
                        : request.Profile?.User;

                    result.Add(new ConversationSummaryDto(
                        requestId,
                        otherUser?.Name ?? "Unknown",
                        otherUser?.ImageUrl,
                        lastMessage?.Content ?? lastMessage?.MessageType.ToString(),
                        lastMessage?.SentAt,
                        unread));
                }

                return result.OrderByDescending(c => c.LastMessageTime).ToList();
            }

            private static ChatMessageResponseDto MapToDto(ChatMessage m) => new(
                m.Id, m.RequestId, m.SenderId,
                m.Sender?.Name ?? "Unknown",
                m.Content, m.AttachmentUrls,
                m.MessageType, m.DurationSeconds,
                m.IsRead, m.ReadAt, m.SentAt);
        }
    
}
