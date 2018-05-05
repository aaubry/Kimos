#addin "Cake.SqlTools"
#tool "nuget:?package=GitVersion.CommandLine"

using System.Text.RegularExpressions;

var target = Argument("target", "Build");
var configuration = Argument("configuration", "Release");

Task("Restore")
    .Does(() =>
    {
        DotNetCoreRestore();
    });

Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
    {
        DotNetCoreBuild("Kimos.sln", new DotNetCoreBuildSettings
        {
            NoRestore = true,
            Configuration = configuration,
        });
    });

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
    {
        if (!AppVeyor.IsRunningOnAppVeyor)
        {
            StartProcess("docker-compose", new ProcessSettings
            {
                WorkingDirectory = Directory("Tests/Kimos.Tests"),
                Arguments = new ProcessArgumentBuilder()
                    .Append("up")
                    .Append("--force-recreate")
                    .Append("-d")
            });
        }

        EnsureSqlServerTestDatabase();
        EnsurePostgreSqlTestDatabase();

        DotNetCoreTest("Tests/Kimos.Tests/Kimos.Tests.csproj", new DotNetCoreTestSettings
        {
            NoBuild = true,
            NoRestore = true,
            Configuration = configuration,            
            EnvironmentVariables = new Dictionary<string, string>
            {
                { "ConnectionStrings:SqlServer", SqlServerConnectionString("kimos") },
                { "ConnectionStrings:PostgreSql", PostgreSqlConnectionString("kimos") },
            }
        });
    })
    .Finally(() =>
    {  
        if (!AppVeyor.IsRunningOnAppVeyor)
        {
            StartProcess("docker-compose", new ProcessSettings
            {
                WorkingDirectory = Directory("Tests/Kimos.Tests"),
                Arguments = new ProcessArgumentBuilder()
                    .Append("kill")
            });

            StartProcess("docker-compose", new ProcessSettings
            {
                WorkingDirectory = Directory("Tests/Kimos.Tests"),
                Arguments = new ProcessArgumentBuilder()
                    .Append("rm")
                    .Append("-v")
                    .Append("-f")
            });
        }
    });

Task("Pack")
    .IsDependentOn("Test")
    .Does(() =>
    {
        var version = GitVersion(new GitVersionSettings
        {
            UpdateAssemblyInfo = false,
            NoFetch = true,
        });

        if (AppVeyor.IsRunningOnAppVeyor)
        {
            AppVeyor.UpdateBuildVersion($"{version.NuGetVersion}/{AppVeyor.Environment.Build.Number}");
        }

        DotNetCorePack("Kimos", new DotNetCorePackSettings
        {
            NoBuild = true,
            NoRestore = true,
            Configuration = configuration,
            ArgumentCustomization = args => args.Append($"/p:Version={version.NuGetVersion}"),
        });
    });

RunTarget(target);


string GetDockerHostName()
{
    var dockerHost = EnvironmentVariable("DOCKER_HOST");
    if (dockerHost != null)
    {
        return Regex.Match(dockerHost, @"^tcp://([^:]+)").Groups[1].Value;
    }

    return "localhost";
}

void Retry(int maxRetries, int retryDelay, Action action)
{
    var retries = 0;
    while (true)
    {
        try
        {
            action();
            break;
        }
        catch (Exception ex)
        {
            if (++retries == maxRetries)
            {
                throw;
            }

            Warning(ex.Message);
            System.Threading.Thread.Sleep(retryDelay);
            Information("Retrying...");
        }
    }
}

string SqlServerConnectionString(string databaseName) => AppVeyor.IsRunningOnAppVeyor
    ? $"Server=(local)\\SQL2017; Database={databaseName}; User Id=sa; Password=Password12!;"
    : $"Server={GetDockerHostName()},11433; Database={databaseName}; User Id=sa; Password=KimosTestsPassw0rd!;";

void EnsureSqlServerTestDatabase()
{
    Information("Creating the SQL Server database");

    Retry(10, 500, () => ExecuteSqlQuery("CREATE DATABASE kimos", new SqlQuerySettings
    {
        Provider = "MsSql",
        ConnectionString = SqlServerConnectionString("master"),
    }));

    DotNetCoreTool(
        "Tests/Kimos.Tests.SqlServer/Kimos.Tests.SqlServer.csproj",
        "ef",
        "database update",
        new DotNetCoreToolSettings
        {
            EnvironmentVariables = new Dictionary<string, string>
            {
                { "ConnectionStrings:SqlServer", SqlServerConnectionString("kimos") }
            }
        }
    );
}

string PostgreSqlConnectionString(string databaseName) => AppVeyor.IsRunningOnAppVeyor
    ? $"User ID=postgres;Password=Password12!;Host=localhost;Port=5432;Database={databaseName};Pooling=true;"
    : $"User ID=postgres;Password=sa;Host={GetDockerHostName()};Port=15432;Database={databaseName};Pooling=true;";

void EnsurePostgreSqlTestDatabase()
{
    Information("Creating the PostgreSQL database");

    DotNetCoreTool(
        "Tests/Kimos.Tests.PostgreSql/Kimos.Tests.PostgreSql.csproj",
        "ef",
        "database update",
        new DotNetCoreToolSettings
        {
            EnvironmentVariables = new Dictionary<string, string>
            {
                { "ConnectionStrings:PostgreSql", PostgreSqlConnectionString("kimos") }
            }
        }
    );
}
