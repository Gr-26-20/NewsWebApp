
using Microsoft.EntityFrameworkCore;
using NewsWebApp.Data;
using NewsWebApp.Models;

namespace NewsWebApp.Services;

public class WeatherArticleGenerationService
{
    private readonly WeatherService _weatherService;
    private readonly SmhiWeatherWarningService _smhiWeatherWarningService;
    private readonly WeatherEditorialAiService _weatherEditorialAiService;
    private readonly WeatherImageAiService _weatherImageAiService;
    private readonly IFileService _fileService;
    private readonly ApplicationDbContext _context;

    public WeatherArticleGenerationService(
        WeatherService weatherService,
        SmhiWeatherWarningService smhiWeatherWarningService,
        WeatherEditorialAiService weatherEditorialAiService,
        WeatherImageAiService weatherImageAiService,
        IFileService fileService,
        ApplicationDbContext context)
    {
        _weatherService = weatherService;
        _smhiWeatherWarningService = smhiWeatherWarningService;
        _weatherEditorialAiService = weatherEditorialAiService;
        _weatherImageAiService = weatherImageAiService;
        _fileService = fileService;
        _context = context;
    }

    public async Task<WeatherArticle> GenerateAndSaveAsync(
        CancellationToken cancellationToken)
    {
        // Get forecasts representing the three main regions of Sweden.
        var regionalForecasts = await Task.WhenAll(
            _weatherService.Get24HourForecastAsync("Gothenburg"),
            _weatherService.Get24HourForecastAsync("Stockholm"),
            _weatherService.Get24HourForecastAsync("Umea")
        );

        var forecast = regionalForecasts
            .SelectMany(region => region)
            .ToList();

        // Get active official SMHI warnings.
        var warnings =
            await _smhiWeatherWarningService.GetActiveWarningsAsync();

        // Generate the national editorial and image brief.
        var editorial = await _weatherEditorialAiService
            .GenerateEditorialAsync(
                "Sweden",
                forecast,
                warnings,
                cancellationToken);

        // Extract IMAGE BRIEF from the editorial.
        const string imageBriefMarker = "IMAGE BRIEF:";

        var imageBriefIndex = editorial.IndexOf(
            imageBriefMarker,
            StringComparison.OrdinalIgnoreCase);

        if (imageBriefIndex < 0)
        {
            throw new InvalidOperationException(
                "The generated editorial did not contain an IMAGE BRIEF section.");
        }

        var imageBrief = editorial[
            (imageBriefIndex + imageBriefMarker.Length)..].Trim();

        if (string.IsNullOrWhiteSpace(imageBrief))
        {
            throw new InvalidOperationException(
                "The generated editorial contained an empty IMAGE BRIEF section.");
        }


        // Build the image-generation prompt from the selected weather story.
        var prompt = $"""
            Create one realistic editorial weather photograph for a Swedish
            national news website, illustrating the following weather story.

            IMAGE BRIEF:
            {imageBrief}

            Follow the geographical setting and weather conditions described
            in the image brief. Depict only conditions supported by the brief.
            Do not automatically use Gothenburg or any other city unless the
            brief calls for it.

            Show ONE single, continuous photographic scene captured from
            ONE camera viewpoint. The landscape must form one coherent,
            uninterrupted environment from the left edge to the right edge.

            ABSOLUTELY NO collages, triptychs, split screens, diptychs,
            multiple panels, side-by-side photographs, tiled scenes,
            composite layouts, or visible divisions between scenes.

            Do not attempt to illustrate several Swedish regions at once.
            Focus on the single most newsworthy weather event described
            in the image brief.

            Photographic style:
            The image must look like an ordinary, unedited snapshot taken by
            a person standing at ground level and holding an older iPhone.

            Use a human eye-level viewpoint, with slightly imperfect
            framing and ordinary smartphone image quality. Allow mildly uneven
            exposure, limited dynamic range, subtle image noise and slightly
            soft details. Use natural, unprocessed colours and realistic
            lighting.

            Avoid professional landscape photography, cinematic composition,
            dramatic colour grading, HDR effects, glossy travel-magazine
            aesthetics, and artificial-looking perfection.

            Do not use drone, aerial, bird's-eye or helicopter viewpoints.
            The camera must appear to be held by a person standing on the ground.

            Compose a wide panoramic photograph intended for a final aspect
            ratio of 1500x417 pixels. Keep the horizon and important visual
            details within the central horizontal band. Extend the same
            continuous scenery naturally to both edges.

            This must look like one genuine landscape photograph, not a
            collection of images or an infographic.

            Do not include text, logos, watermarks or graphic overlays.
            """;

        // Generate the weather image.
        var imageBytes = await _weatherImageAiService
            .GenerateImageAsync(prompt, cancellationToken);

        // Upload the image to blob storage.
        var imageUrl = await _fileService.UploadWeatherImageAsync(
            imageBytes,
            DateOnly.FromDateTime(DateTime.Now),
            cancellationToken);

        // Save the editorial and image URL in SQL.
        var article = new WeatherArticle
        {
            City = "Sweden",
            ForecastDate = DateTime.Today,
            EditorialText = editorial,
            ImageUrl = imageUrl,
            GeneratedAt = DateTime.UtcNow
        };

        _context.WeatherArticles.Add(article);
        await _context.SaveChangesAsync(cancellationToken);

        return article;
    }
}


