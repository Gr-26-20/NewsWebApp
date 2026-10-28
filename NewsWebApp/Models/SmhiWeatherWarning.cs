using System.Text.Json.Serialization;

namespace NewsWebApp.Models
{
    public class SmhiWeatherWarning
    {
        public int Id { get; set; }

        public bool NormalProbability { get; set; }

        public SmhiLocalizedValue? Event { get; set; }

        public SmhiLocalizedValue? MhoClassification { get; set; }

        public List<SmhiWarningDescription> Descriptions { get; set; }
            = new();

        public List<SmhiWarningArea> WarningAreas { get; set; }
            = new();
    }

    public class SmhiWarningArea
    {
        public int Id { get; set; }

        public DateTimeOffset? ApproximateStart { get; set; }

        public DateTimeOffset? ApproximateEnd { get; set; }

        public DateTimeOffset? Published { get; set; }

        public DateTimeOffset? Created { get; set; }

        public bool NormalProbability { get; set; }

        public SmhiLocalizedValue? AreaName { get; set; }

        public SmhiLocalizedValue? WarningLevel { get; set; }

        public SmhiLocalizedValue? EventDescription { get; set; }

        public List<SmhiLocalizedValue> AffectedAreas { get; set; }
            = new();

        public List<SmhiWarningDescription> Descriptions { get; set; }
            = new();
    }

    public class SmhiWarningDescription
    {
        public SmhiLocalizedValue? Title { get; set; }

        public SmhiLocalizedValue? Text { get; set; }
    }

    public class SmhiLocalizedValue
    {
        public int? Id { get; set; }

        public string? Code { get; set; }

        public string? En { get; set; }

        public string? Sv { get; set; }
    }
}