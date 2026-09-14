namespace NewsWebApp.Models
{
    public class FileUploadModel

    {
        public string Slug { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Summary { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public IFormFile File { get; set; }



    }
}
