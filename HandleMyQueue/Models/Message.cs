using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HandleMyQueue.Models
{
    public class Message
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string MessageId { get; set; }

        public BsonDateTime CreatedAt { get; set; }

        public string UserName { get; set; }

        public string QueueName { get; set; }

        public BsonBoolean Processed { get; set; }

        public BsonDocument MessageContent { get; set; }
    }
}