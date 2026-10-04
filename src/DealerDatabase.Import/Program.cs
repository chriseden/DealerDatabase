using DealerDatabase.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DealerDatabase.Services.Interfaces;
using DealerDatabase.Services.Services;
using DealerDatabase.Services.Configuration;
using DealerDatabase.DataAccess.Interfaces;
using DealerDatabase.DataAccess.Repository;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDealerDatabase();
builder.Services.DataReaderSetup();
builder.Services.AddScoped<IMatchingService, MatchingService>();
builder.Services.AddScoped<IDealerRepository, DealerRepository>();
builder.Services.AddScoped<IIngestionService, IngestionService>();
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddTransient<DataOrchestrator>();


using var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();

await using (var scope = host.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DealerDbContext>();
    await db.Database.MigrateAsync();
}

logger.LogInformation("Database: {DatabaseFile}", SolutionPaths.DatabaseFile);
logger.LogInformation("Data folder: {DataDirectory}", SolutionPaths.DataDirectory);

var orchestrator = host.Services.GetRequiredService<DataOrchestrator>();

try
{
    await orchestrator.ExecutePipelineAsync(SolutionPaths.DataDirectory);
    logger.LogInformation("Pipeline execution completed successfully.");
}
catch (Exception ex)
{
    logger.LogCritical(ex, "Pipeline execution failed with an unhandled exception.");
    Environment.ExitCode = 1;
}
finally
{
    await host.StopAsync();
}
