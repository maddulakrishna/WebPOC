namespace WebApplication1.Models
{
    public class ImageResponse
    {
        public List<ImageModel> images { get; set; }
        public List<ImageModel> disimages { get; set; }
        public int totalnoofimages { get { return images != null ? images.Count : 0; } }
        public string error { get; set; }

        public int TotalWordCount { get; set; }
        public Dictionary<string, int> TopWords { get; set; }
    }
}
