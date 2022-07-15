namespace _MediaInfoService.Models
{
    public class HealthResult
    {
        public string? Name { get; set; }
        public string? Status { get; set; }
        public TimeSpan? Duration { get; set; }
        public ICollection<HealthServiceStatus>? Services { get; set; }
    }
}
