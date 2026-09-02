namespace TaskManagement.Tests
{
    [CollectionDefinition(Name)]
    public class DatabaseCollection : ICollectionFixture<PostgreSqlFixture>
    {
        public const string Name = "Database";
    }
}
