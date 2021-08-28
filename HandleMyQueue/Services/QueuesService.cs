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

namespace HandleMyQueue.Services
{
    public interface IQueuesService
    {
        public Task<List<QueueDto>> GetAllQueuesForUser(string username);
        public Task<long> CountAllQueuesForUser(string username);
        public void Create(QueueDto queueDto, string username);
    }
    public class QueuesService : IQueuesService
    {
        private readonly IMongoCollection<Queue> _queuesCollection;

        public QueuesService(IQueuesDatabaseSettings databaseSettings)
        {
            var client = new MongoClient(databaseSettings.ConnectionString);
            var database = client.GetDatabase(databaseSettings.DatabaseName);

            _queuesCollection = database.GetCollection<Queue>(databaseSettings.QueuesCollectionName);
        }
        public async Task<List<QueueDto>> GetAllQueuesForUser(string username)
        {
            var findByUserIdFilter = Builders<Queue>.Filter.Eq("UserName", username);
            var docs = _queuesCollection.Find(findByUserIdFilter).ToList();
            var queueDtos = new List<QueueDto>();
            foreach (var doc in docs)
            {
                var queueDto = new QueueDto()
                {
                    QueueContent = JsonDocument.Parse(doc.QueueContent.ToJson()),
                    QueueId = doc.QueueId
                };
                queueDtos.Add(queueDto);
            }

            return queueDtos;
        }

        public async Task<long> CountAllQueuesForUser(string username)
        {
            var findByUserIdFilter = Builders<Queue>.Filter.Eq("UserName", username);
            return await _queuesCollection.CountDocumentsAsync(findByUserIdFilter);
        }

        public void Create(QueueDto queueDto, string username)
        {
            var queueContent = queueDto.QueueContent.RootElement.ToString();
            var queueContentBson = BsonDocument.Parse(queueContent);
            var queue = new Queue
            {
                UserName = username,
                Processed = false,
                QueueName = queueDto.QueueName,
                QueueContent = queueContentBson
            };
            // var dotNetObj = BsonTypeMapper.MapToDotNetValue(queue.QueueContent);
            _queuesCollection.InsertOne(queue);
        }
    }
}