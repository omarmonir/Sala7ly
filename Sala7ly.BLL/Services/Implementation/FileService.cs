using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Sala7ly.BLL.Services.Abstraction;

namespace Sala7ly.BLL.Services.Implementation
{
    public class FileService : IFileService
    {
        private readonly IFilePathProvider _pathProvider;

        public FileService(IFilePathProvider pathProvider, IHttpContextAccessor httpContextAccessor)
        {
            _pathProvider = pathProvider;
            _baseUploadPath = Path.Combine(_pathProvider.GetWebRootPath(), "Images");
            _httpContextAccessor = httpContextAccessor;
            if (!Directory.Exists(_baseUploadPath))
            {
                Directory.CreateDirectory(_baseUploadPath);
            }
        }
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _baseUploadPath;


        public async Task<string> SaveFileAsync(IFormFile file, string category)

        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException("نوع الملف غير مدعوم، يُسمح فقط بـ JPG, JPEG, PNG");
            }

            const long maxFileSize = 2 * 1024 * 1024; //2MB
            if (file.Length > maxFileSize)
            {
                throw new InvalidOperationException("حجم الملف يتجاوز الحد الأقصى المسموح به 2MB");
            }

            var categoryPath = Path.Combine(_baseUploadPath, category);
            if (!Directory.Exists(categoryPath))
            {
                Directory.CreateDirectory(categoryPath);
            }

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(categoryPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null)
            {
                throw new InvalidOperationException("لا يمكن تحديد عنوان الموقع، تأكد من تمرير HttpContextAccessor بشكل صحيح");
            }
            var baseUrl = $"{request?.Scheme}://{request?.Host}";

            return $"{baseUrl}/Images/{category}/{fileName}";
        }

        public async Task DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
            {
                throw new ArgumentException("مسار الملف غير صالح");
            }

            var relativePath = fileUrl.Replace($"{_httpContextAccessor.HttpContext?.Request.Scheme}://{_httpContextAccessor.HttpContext?.Request.Host}/", "");


            var fullPath = Path.Combine(_pathProvider.GetWebRootPath(), relativePath);

            if (File.Exists(fullPath))
            {
                try
                {
                    await Task.Run(() => File.Delete(fullPath));
                }
                catch (Exception ex)
                {
                    throw new IOException($"حدث خطأ أثناء حذف الملف: {ex.Message}");
                }
            }

        }
    }
}