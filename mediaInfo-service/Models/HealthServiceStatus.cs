namespace _MediaInfoService.Models
{
    public class HealthServiceStatus
    {
        public string? Key { get; set; }
        public string? Description { get; set; }
        public TimeSpan? Duration { get; set; }
        public string? Status { get; set; }
        public string? Error { get; set; }
        public ICollection<KeyValuePair<string, object>>? Data { get; set; }
    }
}
