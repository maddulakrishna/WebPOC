using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using WebApplication1.Models;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.Extensions.Caching.Memory;
using System.Web;
using System.Text;
using System.Reflection.Metadata;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IImageService _imageService;
       

        public HomeController(ILogger<HomeController> logger, IImageService imageService)
        {
            _logger = logger;
            _imageService = imageService;
        }


        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }



        [HttpPost]
        public ActionResult FetchImages(string url, int? page)
        {
            ImageResponse imageResponse = new ImageResponse();
            try
            {
                if (!_imageService.IsValidUrl(url))
                {
                    imageResponse.error = "Invalid URL. Please enter a valid URL.";
                    return Json(imageResponse);
                }

                imageResponse = _imageService.generateImageResponse(url, page);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }


            return Json(imageResponse);
        }

       

    }
}
