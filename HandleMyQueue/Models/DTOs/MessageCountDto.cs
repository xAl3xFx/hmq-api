namespace HandleMyQueue.Models.DTOs
{
    public class MessageCountDto
    {
        public long TotalCount { get; set; }
        public long ProcessedCount { get; set; }
        public long UnProcessedCount { get; set; }
    }
}