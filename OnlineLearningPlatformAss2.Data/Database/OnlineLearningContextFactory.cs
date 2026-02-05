using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace OnlineLearningPlatformAss2.Data.Database;

public class OnlineLearningContextFactory : IDesignTimeDbContextFactory<OnlineLearningContext>
{
    public OnlineLearningContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();
        var configDir = FindConfigurationDirectory(basePath);

        var configuration = new ConfigurationBuilder()
            .SetBasePath(configDir)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException(
                "The connection string 'DefaultConnection' was not found in any appsettings.json files. " +
                $"Searched starting from: {basePath}. Ensure your web project's appsettings.json contains the connection string or run the command from the solution root.");

        var optionsBuilder = new DbContextOptionsBuilder<OnlineLearningContext>();
        optionsBuilder.UseSqlServer(connectionString);
        return new OnlineLearningContext(optionsBuilder.Options);
    }

    private static string FindConfigurationDirectory(string startDirectory)
    {
        var dir = new DirectoryInfo(startDirectory);
        for (int i = 0; i < 10 && dir != null; i++)
        {
            var appsettingsPath = Path.Combine(dir.FullName, "appsettings.json");
            if (File.Exists(appsettingsPath))
                return dir.FullName;

            // Also check common web project folder sibling
            var webProjPath = Path.Combine(dir.FullName, "OnlineLearningPlatformAss2.RazorWebApp");
            var webAppsettings = Path.Combine(webProjPath, "appsettings.json");
            if (File.Exists(webAppsettings))
                return webProjPath;

            dir = dir.Parent;
        }

        // fallback to start directory
        return startDirectory;
    }
}
