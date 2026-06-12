using Microsoft.AspNetCore.Http;

namespace Sala7ly.BLL.Services.Abstraction
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file, string category);
        Task DeleteFileAsync(string relativePath);
    }
}
