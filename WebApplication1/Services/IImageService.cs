using WebApplication1.Models;

namespace WebApplication1.Services
{
    public interface IImageService
    {
        ImageResponse generateImageResponse(string url, int? page);
        bool IsValidUrl(string url);
    }
}
