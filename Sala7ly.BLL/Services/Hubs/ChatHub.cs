using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Sala7ly.BLL.DTOs.ChatDTOs;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Enums;

namespace Sala7ly.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;

        public ChatHub(IChatService chatService) => _chatService = chatService;

        private string UserId =>
            Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public async Task JoinRequest(int requestId)
        {
            if (!await _chatService.CanAccessRequestAsync(UserId, requestId))
            {
                await Clients.Caller.SendAsync("Error", "ليس لديك صلاحية للوصول إلى هذه المحادثة.");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(requestId));
            await Clients.Caller.SendAsync("Joined", requestId);
        }

        public async Task LeaveRequest(int requestId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(requestId));
        }

        public async Task SendTextMessage(int requestId, string content)
        {
            var dto = new SendMessageDto(requestId, content, ChatMessageType.text, null);
            var saved = await _chatService.SaveMessageAsync(UserId, dto, null);

            await Clients.Group(GroupName(requestId))
                .SendAsync("ReceiveMessage", saved);
        }

        public async Task MarkRead(int requestId)
        {
            await _chatService.MarkAsReadAsync(requestId, UserId);
            await Clients.Group(GroupName(requestId))
                .SendAsync("MessagesRead", new { requestId, readBy = UserId });
        }

        public async Task Typing(int requestId)
        {
            await Clients.OthersInGroup(GroupName(requestId))
                .SendAsync("UserTyping", new { userId = UserId, requestId });
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }

        private static string GroupName(int requestId) => $"request_{requestId}";
    }
}
