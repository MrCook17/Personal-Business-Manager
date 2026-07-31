namespace PersonalBusinessManager.IntegrationTests;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class MariaDbTestGroup
{
    public const string Name = "MariaDB integration";
}
