namespace GzipFeeder.FeederService
{
    public interface IFeed
    {
        public Task FeedAsync<T>(List<T> payload, string tableName, string mappingName);
    }
}
