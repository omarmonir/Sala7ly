using System;
using Sala7ly.DAL.Enums;

namespace Sala7ly.BLL.DTOs.NotificationDTOs
{
    public class NotificationResponseDto
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string? DeepLink { get; set; }
        public bool IsRead { get; set; }
        public DateTime SentAt { get; set; }
    }
}