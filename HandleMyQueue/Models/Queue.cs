using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HandleMyQueue.Models
{
    public class Queue
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string QueueId { get; set; }

        public string UserName { get; set; }

        public string QueueName { get; set; }

        public BsonBoolean Processed { get; set; }

        public BsonDocument QueueContent { get; set; }

    }
}