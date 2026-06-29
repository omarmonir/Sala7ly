using Sala7ly.BLL.DTOs.ServiceRequestDTOs;
using Sala7ly.DAL.Entities;

namespace Sala7ly.BLL.Services.Abstraction
{
    public interface IEmbeddingService
    {
        Task<float[]> GetEmbeddingAsync(string text);
        float CosineSimilarity(float[] a, float[] b);      // ← add
        string BuildTechnicianText(TechnicianProfile t);    // ← add
    }
}