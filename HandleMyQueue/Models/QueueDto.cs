using System.Text.Json;

namespace HandleMyQueue.Models
{
    public class QueueDto
    {
        public string QueueId { get; set; }

        public string UserName { get; set; }

        public string QueueName { get; set; }

        public bool Processed { get; set; }

        public JsonDocument QueueContent { get; set; }
    }
}