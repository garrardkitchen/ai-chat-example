using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.AddAzureQueueClient("queues");
builder.AddAzureBlobClient("blobs");


builder.ConfigureFunctionsWebApplication();

// create blob containers and queue
using var tempProvider = builder.Services.BuildServiceProvider();
await using var scope = tempProvider.CreateAsyncScope();
var blobClients = scope.ServiceProvider.GetServices<BlobServiceClient>();
await blobClients.FirstOrDefault().GetBlobContainerClient("uploads").CreateIfNotExistsAsync();
await blobClients.FirstOrDefault().GetBlobContainerClient("pdf").CreateIfNotExistsAsync();
var queueClients = scope.ServiceProvider.GetServices<QueueServiceClient>();
await queueClients.FirstOrDefault().GetQueueClient("new-pdf-queue").CreateIfNotExistsAsync();

builder.Build().Run();

