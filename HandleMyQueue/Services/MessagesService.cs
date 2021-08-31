using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HandleMyQueue.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Text.Json;
using HandleMyQueue.Models.DTOs;

namespace HandleMyQueue.Services
{
    public interface IMessagesService
    {
        public Task<List<MessageDto>> GetAllMessagesForUser(string username);
        public Task<MessageCountDto> CountAllMessagesForUser(string username);
        public void Create(MessageDto messageDto, string username);
    }
    public class MessagesService : IMessagesService
    {
        private readonly IMongoCollection<Message> _queuesCollection;

        public MessagesService(IMessagesDatabaseSettings databaseSettings)
        {
            var client = new MongoClient(databaseSettings.ConnectionString);
            var database = client.GetDatabase(databaseSettings.DatabaseName);

            _queuesCollection = database.GetCollection<Message>(databaseSettings.MessagesCollectionName);

            // var indexKeysDefinition = Builders<Message>.IndexKeys.Ascending("CreatedAt");
            // var indexOptions = new CreateIndexOptions<Message>() { ExpireAfter = new TimeSpan(1, 0,0), PartialFilterExpression = };
            // var indexModel = new CreateIndexModel<Message>(indexKeysDefinition, indexOptions);
            // _queuesCollection.Indexes.CreateOne(indexModel);

        }
        public async Task<List<MessageDto>> GetAllMessagesForUser(string username)
        {
            var findByUserIdFilter = Builders<Message>.Filter.Eq("UserName", username);
            var docs = _queuesCollection.Find(findByUserIdFilter).ToList();
            var messages = new List<MessageDto>();

            foreach (var doc in docs)
            {
                var messageDto = new MessageDto
                {
                    CreatedAt = doc.CreatedAt.ToLocalTime(),
                    QueueName = doc.QueueName,
                    MessageContent = JsonDocument.Parse(doc.MessageContent.ToJson()),
                    MessageId = doc.MessageId
                };
                messages.Add(messageDto);
            }

            return messages;
        }

        public async Task<MessageCountDto> CountAllMessagesForUser(string username)
        {
            var findByUsernameProcessedFilter = Builders<Message>.Filter.And(Builders<Message>.Filter.Eq("UserName", username), Builders<Message>.Filter.Eq("Processed", true));
            var findByUsernameUnProcessedFilter = Builders<Message>.Filter.And(Builders<Message>.Filter.Eq("UserName", username), Builders<Message>.Filter.Eq("Processed", false));
            var processedCount = await _queuesCollection.CountDocumentsAsync(findByUsernameProcessedFilter);
            var unProcessedCount = await _queuesCollection.CountDocumentsAsync(findByUsernameUnProcessedFilter);
            var totalCount = processedCount + unProcessedCount;
            return new MessageCountDto
            {
                ProcessedCount = processedCount,
                UnProcessedCount = unProcessedCount,
                TotalCount = totalCount
            };

        }

        public void Create(MessageDto messageDto, string username)
        {
            var messageContent = messageDto.MessageContent.RootElement.ToString();
            var messageContentBson = BsonDocument.Parse(messageContent);
            var queue = new Message
            {
                CreatedAt = DateTime.Now,
                UserName = username,
                Processed = false,
                QueueName = messageDto.QueueName,
                MessageContent = messageContentBson
            };
            // var dotNetObj = BsonTypeMapper.MapToDotNetValue(queue.MessageContent);
            _queuesCollection.InsertOne(queue);
        }
    }
}