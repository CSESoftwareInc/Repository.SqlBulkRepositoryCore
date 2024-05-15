using Microsoft.Extensions.Configuration;

namespace CSESoftware.Repository.SqlBulkRepositoryCore.TestProject;

public class Environment
{
    private readonly IConfiguration _configuration;

    public Environment()
    {
        _configuration = new ConfigurationBuilder()
            .AddJsonFile(@"appsettings.json", false, true)
            .AddEnvironmentVariables()
            .AddUserSecrets<Environment>()
            .Build();
    }

    public string GetConnectionString()
    {
        return _configuration["ConnectionString"] ?? "";
    }
}
