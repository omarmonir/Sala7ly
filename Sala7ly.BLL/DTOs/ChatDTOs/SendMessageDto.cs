using System;
using System.Collections.Generic;
using System.Text;
using Sala7ly.DAL.Enums;

namespace Sala7ly.BLL.DTOs.ChatDTOs
{
    public record SendMessageDto(
    int RequestId,
    string? Content,                   
    ChatMessageType MessageType,        
    int? DurationSeconds              
    );
}
