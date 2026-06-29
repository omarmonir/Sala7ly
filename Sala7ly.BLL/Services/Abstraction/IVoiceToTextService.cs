using Microsoft.AspNetCore.Http;

namespace Sala7ly.BLL.Services.Abstraction
{
    // Sala7ly.BLL/AI/Interfaces/IVoiceToTextService.cs
    public interface IVoiceToTextService
    {
        Task<string> TranscribeAsync(IFormFile audioFile);
    }
}