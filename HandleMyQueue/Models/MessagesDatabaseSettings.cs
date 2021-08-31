namespace HandleMyQueue.Models
{
    public class MessagesDatabaseSettings : IMessagesDatabaseSettings
    {
        public string MessagesCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public interface IMessagesDatabaseSettings
    {
        string MessagesCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}
