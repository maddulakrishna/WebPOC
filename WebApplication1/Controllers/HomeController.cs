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

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMemoryCache _memoryCache;

        public HomeController(ILogger<HomeController> logger, IMemoryCache memoryCache)
        {
            _logger = logger;
            _memoryCache = memoryCache;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private const int PageSize = 10; // Number of images per page       

        [HttpPost]
        public ActionResult FetchImages(string url, int? page)
        {
            ImageResponse imageResponse = new ImageResponse();
            if (!IsValidUrl(url))
            {
                imageResponse.error = "Invalid URL. Please enter a valid URL.";
                return Json(imageResponse);
            }

            imageResponse = generateImageResponse(url, page);


            return Json(imageResponse);
        }

        private ImageResponse generateImageResponse(string url, int? page)
        {
            ImageResponse imageResponse = new ImageResponse();
            List<ImageModel> images = null;
            string surl = string.Empty;
            if (_memoryCache.TryGetValue("url", out string urlBytes))
            {
                surl = urlBytes;
            }
            if (surl == url)
            {
                if (_memoryCache.TryGetValue("imageResponse", out ImageResponse imagesdata))
                {
                    imageResponse = imagesdata;
                }
            }
            else
            {
                imageResponse = GetImagesFromUrl(url);               
                _memoryCache.Set("imageResponse", imageResponse);
            }


         //   imageResponse.totalnoofimages = imageResponse.images != null ? imageResponse.images.Count : 0;

            if (imageResponse.images != null && imageResponse.images.Count > 0)
            {
                int pageNumber = page ?? 1;
                imageResponse.disimages = imageResponse.images.Skip((pageNumber - 1) * PageSize).Take(PageSize).ToList();                 
            }
            else
            {
                imageResponse.error = "Images are not available in the page.";
            }



            _memoryCache.Set("url", url);
            return imageResponse;
        }

        /// <summary>
        /// Check the URL is valid or not
        /// </summary>
        /// <param name="url">page url</param>
        /// <returns></returns>
        private bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }

        private ImageResponse GetImagesFromUrl(string url)
        {
            List<ImageModel> images = new List<ImageModel>();
            ImageResponse res = new ImageResponse();
            try
            {
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load(url);

                var imgcollection = doc.DocumentNode.SelectNodes("//img");
                if (imgcollection != null)
                {
                    foreach (HtmlNode imgNode in doc.DocumentNode.SelectNodes("//img"))
                    {
                        string imageUrl = imgNode.GetAttributeValue("src", null);
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            images.Add(new ImageModel { Source = imageUrl.StartsWith('/') ? url.TrimEnd('/') + imageUrl : imageUrl });
                        }
                    }
                }

                var wordList = doc.DocumentNode.InnerText
                                 .Split(' ', '\t', '\n', '\r')
                                 .Where(s => !string.IsNullOrWhiteSpace(s))
                                 .Select(s => s.ToLowerInvariant());

                var wordCounts = wordList.GroupBy(w => w)
                                         .ToDictionary(g => g.Key, g => g.Count());

                var topWords = wordCounts.OrderByDescending(w => w.Value)
                                         .Take(10)
                                         .ToDictionary(w => w.Key, w => w.Value);

                res.TopWords = topWords;
                res.TotalWordCount = wordList.Count();
            }
            catch (Exception ex)
            {
                // Handle exception
            }
           res.images = images;
           
            return res;
        }

    }
}
