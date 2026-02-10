using DataSyncService.Configuration;
using DataSyncService.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Add region configuration
builder.Services.Configure<RegionConfiguration>(
    builder.Configuration.GetSection(RegionConfiguration.ConfigSection));

// Register sync services
builder.Services.AddScoped<ISyncService, SyncService>();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
