using Sala7ly.BLL.Services.Abstraction;

namespace Sala7ly.API.Models
{
    public class WebHostEnvironmentPathProvider : IFilePathProvider
    {
        private readonly IWebHostEnvironment _env;

        public WebHostEnvironmentPathProvider(IWebHostEnvironment env)
        {
            _env = env;
        }

        public string GetWebRootPath()
        {
            return _env.WebRootPath
                   ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }
    }
}
