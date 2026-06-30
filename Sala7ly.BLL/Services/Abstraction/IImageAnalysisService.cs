using Microsoft.AspNetCore.Http;
using Sala7ly.BLL.DTOs.AiDTOs;

namespace Sala7ly.BLL.Services.Abstraction
{
    public interface IImageAnalysisService
    {
        Task<ImageAnalysisDto> AnalyzeImageAsync(string base64Image, string mediaType = "image/jpeg");

        Task<ImageAnalysisDto> AnalyzeRequestImageAsync(int requestId, string imageUrl, string? userId = null);

        Task<ImageAnalysisDto> AnalyzeMultipleImagesAsync(int requestId, List<string> imageUrls, string? userId = null);
    }
}