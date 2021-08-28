namespace HandleMyQueue.Models
{
    public class QueuesDatabaseSettings : IQueuesDatabaseSettings
    {
        public string QueuesCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public interface IQueuesDatabaseSettings
    {
        string QueuesCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}
