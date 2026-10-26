
using Azure.Core;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace NewsWebApp.Services
{
    public class WeatherImageAiService
    {
        private readonly HttpClient _httpClient;
        private readonly TokenCredential _credential;
        private readonly IConfiguration _configuration;

        public WeatherImageAiService(
            HttpClient httpClient,
            TokenCredential credential,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _credential = credential;
            _configuration = configuration;
        }

        public async Task<byte[]> GenerateImageAsync(
            string prompt,
            CancellationToken cancellationToken = default)
        {
            var endpoint =
                _configuration["Foundry:ImageEndpoint"];

            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new InvalidOperationException(
                    "The Foundry image endpoint is not configured.");
            }

            var accessToken = await _credential.GetTokenAsync(
                new TokenRequestContext(
                    new[] { "https://ai.azure.com/.default" }),
                cancellationToken);

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                endpoint);

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer", accessToken.Token);

            request.Content = JsonContent.Create(new
            {
                prompt,
                n = 1,
                size = "1536x1024",
                quality = "medium"
            });

            using var response = await _httpClient.SendAsync(
                request,
                cancellationToken);

            var responseBody =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Image generation returned HTTP " +
                    $"{(int)response.StatusCode}: {responseBody}");
            }

            using var document = JsonDocument.Parse(responseBody);

            var data = document.RootElement.GetProperty("data");
            var base64 = data[0].GetProperty("b64_json").GetString();

            if (string.IsNullOrWhiteSpace(base64))
            {
                throw new InvalidOperationException(
                    "The image API returned no image data.");
            }

            var imageBytes = Convert.FromBase64String(base64);

            return await CropToPanoramaAsync(imageBytes);
        }

        private static async Task<byte[]> CropToPanoramaAsync(
        byte[] imageBytes)
        {
            const int targetWidth = 1500;
            const int targetHeight = 417;

            using var input = new MemoryStream(imageBytes);
            using var image = await Image.LoadAsync(input);

            double targetRatio = (double)targetWidth / targetHeight;
            double sourceRatio = (double)image.Width / image.Height;

            Rectangle crop;

            if (sourceRatio > targetRatio)
            {
                // Image is too wide. Crop the sides.
                int cropWidth = (int)Math.Round(
                    image.Height * targetRatio);

                int x = (image.Width - cropWidth) / 2;

                crop = new Rectangle(
                    x, 0, cropWidth, image.Height);
            }
            else
            {
                // Image is too tall. Crop the top and bottom.
                int cropHeight = (int)Math.Round(
                    image.Width / targetRatio);

                int y = (image.Height - cropHeight) / 2;

                crop = new Rectangle(
                    0, y, image.Width, cropHeight);
            }

            image.Mutate(operation => operation
                .Crop(crop)
                .Resize(targetWidth, targetHeight));

            await using var output = new MemoryStream();

            await image.SaveAsPngAsync(output);

            return output.ToArray();
        }
    }
}
