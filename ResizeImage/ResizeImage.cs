using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
        
namespace ResizeImage
{
    public class ResizeImage
    {
        private readonly ILogger<ResizeImage> _logger;

        public ResizeImage(ILogger<ResizeImage> logger)
        {
            _logger = logger;
        }

        [Function(nameof(ResizeImage))]
        public async Task<ResizeImageOutput> Run(
        [BlobTrigger("images/{name}", Connection = "AzureWebJobsStorage")] ReadOnlyMemory<byte> inputBytes,
        string name)
        {
            using var inputImage = new MemoryStream(inputBytes.ToArray(), writable: false);

            // Detect format to choose the right encoder; fall back to JPEG if unknown
            var detectedFormat = SixLabors.ImageSharp.Image.DetectFormat(inputImage);
            IImageEncoder encoder = detectedFormat switch
            {
                JpegFormat => new JpegEncoder { Quality = 85 },
                PngFormat => new PngEncoder(),
                GifFormat => new GifEncoder(),
                BmpFormat => new BmpEncoder(),
                WebpFormat => new WebpEncoder(),
                _ => new JpegEncoder { Quality = 85 }
            };


            inputImage.Position = 0;

            byte[] smallBytes;
            byte[] mediumBytes;

            using (var image = SixLabors.ImageSharp.Image.Load<Rgba32>(inputImage))
            {
                // Small
                using var smallStream = new MemoryStream();
                image.Clone(x => x.Resize(320, 200)).Save(smallStream, encoder);
                smallBytes = smallStream.ToArray();
            }

            inputImage.Position = 0;

            using (var image = SixLabors.ImageSharp.Image.Load<Rgba32>(inputImage))
            {
                // Medium
                using var mediumStream = new MemoryStream();
                image.Clone(x => x.Resize(800, 600)).Save(mediumStream, encoder);
                mediumBytes = mediumStream.ToArray();
            }


            return new ResizeImageOutput
            {
                Small = smallBytes,
                Medium = mediumBytes
            };

            //using var blobStreamReader = new StreamReader(stream);
            //var content = await blobStreamReader.ReadToEndAsync();
            //_logger.LogInformation("C# Blob trigger function Processed blob\n Name: {name} \n Data: {content}", name, content);
        }
    }


    public class ResizeImageOutput
    {
        [BlobOutput("output-images-md/{name}")]
        public byte[] Medium { get; set; }

        [BlobOutput("output-images-sm/{name}")]
        public byte[] Small { get; set; }
    }
}