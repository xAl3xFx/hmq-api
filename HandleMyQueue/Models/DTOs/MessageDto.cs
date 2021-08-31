using System;
using System.Text.Json;

namespace HandleMyQueue.Models.DTOs
{
    public class MessageDto
    {
        public string MessageId { get; set; }

        public DateTime CreatedAt { get; set; }

        public string UserName { get; set; }

        public string QueueName { get; set; }

        public bool Processed { get; set; }

        public JsonDocument MessageContent { get; set; }
    }
}