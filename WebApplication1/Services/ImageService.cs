using HtmlAgilityPack;
using Microsoft.Extensions.Caching.Memory;
using WebApplication1.Controllers;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class ImageService : IImageService
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMemoryCache _memoryCache;
        private const int PageSize = 10; // Number of images per page      
        public ImageService(ILogger<HomeController> logger, IMemoryCache memoryCache) {
            _logger = logger;
            _memoryCache = memoryCache;
        }
        /// <summary>
        /// Generate Images Response
        /// </summary>
        /// <param name="url">url of a page</param>
        /// <param name="page">number of images that want to display</param>
        /// <returns>ImageResponse model</returns>
        public ImageResponse generateImageResponse(string url, int? page)
        {
            ImageResponse imageResponse = new ImageResponse();
            try
            {
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
                    imageResponse = GetImagesandWordsFromUrl(url);
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
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.Message);
            }
            return imageResponse;
        }

        /// <summary>
        /// Check the URL is valid or not
        /// </summary>
        /// <param name="url">page url</param>
        /// <returns></returns>
        public bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }

        /// <summary>
        /// Get the images and word count from the page
        /// </summary>
        /// <param name="url"></param>
        /// <returns>ImageResponse model</returns>
        private ImageResponse GetImagesandWordsFromUrl(string url)
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
                _logger.Log(LogLevel.Error, ex.Message);
            }
            res.images = images;

            return res;
        }
    }
}
