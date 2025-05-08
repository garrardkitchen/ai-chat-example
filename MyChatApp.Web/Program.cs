using Azure.Storage.Blobs;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.VectorData;
//using ModelContextProtocol.Client;
//using ModelContextProtocol.Protocol.Transport;
using MyChatApp.Web.Components;
using MyChatApp.Web.Services;
using MyChatApp.Web.Services.Ingestion;
using OpenAI;
using System.ClientModel;
using Azure.Storage.Blobs.Models;
using Microsoft.SemanticKernel.Connectors.Qdrant;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
var credential = new ApiKeyCredential(builder.Configuration["GitHubModels:Token"] ?? throw new InvalidOperationException("Missing configuration: GitHubModels:Token. See the README for details."));
var openAIOptions = new OpenAIClientOptions()
{
    Endpoint = new Uri("https://models.inference.ai.azure.com")
};

var ghModelsClient = new OpenAIClient(credential, openAIOptions);
// var chatClient = ghModelsClient.GetChatClient("gpt-4o-mini").AsIChatClient();
var chatClient = ghModelsClient.GetChatClient("gpt-4o-mini").AsIChatClient();
var embeddingGenerator = ghModelsClient.GetEmbeddingClient("text-embedding-3-small").AsIEmbeddingGenerator();

// Commented out as now using Qdrant
// var vectorStore = new JsonVectorStore(Path.Combine(AppContext.BaseDirectory, "vector-store"));
// builder.Services.AddSingleton<IVectorStore>(vectorStore);

builder.Services.AddScoped<DataIngestor>();
builder.Services.AddSingleton<SemanticSearch>();
builder.AddSqliteDbContext<IngestionCacheDbContext>("ingestionCache");
builder.Services.AddChatClient(chatClient).UseFunctionInvocation().UseLogging();
builder.Services.AddEmbeddingGenerator(embeddingGenerator);
builder.Services.AddSingleton<McpService>();
builder.AddAzureBlobClient("blobs");
builder.AddAzureQueueClient("queues");
builder.Services.AddHostedService<PdfQueueService>();

// Add Qdrant (have commented out above the JsonVectorStore)
builder.AddQdrantClient("vectordb");
builder.Services.AddSingleton<IVectorStore, QdrantVectorStore>();

var app = builder.Build();
IngestionCacheDbContext.Initialize(app.Services);

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.UseStaticFiles();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// By default, we ingest PDF files from the /wwwroot/Data directory. You can ingest from
// other sources by implementing IIngestionSource.
// Important: ensure that any content you ingest is trusted, as it may be reflected back
// to users or could be a source of prompt injection risk.
await DataIngestor.IngestDataAsync(
    app.Services,
    new PDFDirectorySource(Path.Combine(builder.Environment.WebRootPath, "Data")));

// To ingest from Azure Blob Storage, use AzureBlobPdfSource instead of PDFDirectorySource:
// I want to get BlobContainerClient from DI
var blobServiceClient = app.Services.GetRequiredService<BlobServiceClient>();
var logger = app.Services.GetRequiredService<ILogger<AzureBlobPdfSource>>();
await DataIngestor.IngestDataAsync(app.Services, new AzureBlobPdfSource(blobServiceClient, "pdf", logger) );

var props  = await blobServiceClient.GetPropertiesAsync();

// add your CORS rule
props.Value.Cors.Add(new BlobCorsRule
{
    AllowedOrigins    = "*",
    AllowedMethods    = "GET,OPTIONS",
    AllowedHeaders    = "*",
    ExposedHeaders    = "*",
    MaxAgeInSeconds   = 3600
});

await blobServiceClient.SetPropertiesAsync(props.Value);

app.Run();