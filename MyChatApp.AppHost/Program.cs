var builder = DistributedApplication.CreateBuilder(args);

// You will need to set the connection string to your own value
// You can do this using Visual Studio's "Manage User Secrets" UI, or on the command line:
//   cd this-project-directory
//   dotnet user-secrets set ConnectionStrings:openai "Endpoint=https://models.inference.ai.azure.com;Key=YOUR-API-KEY"
var openai = builder.AddConnectionString("openai");

var ingestionCache = builder.AddSqlite("ingestionCache");

// Add Azure Blob Storage for file uploads
var storage = builder.AddAzureStorage("blobStorage").RunAsEmulator();
var blobs = storage.AddBlobs("blobs");
var queues = storage.AddQueues("queues");

var mcpServer = builder.AddProject<Projects.MyMcpServerHttpApi>("mcp-server").WithExternalHttpEndpoints();

// Not including WithDataVolume here as Blob Storage is using emulator do Blobs not persisted and therefore not exist in vector store
var qdrant = builder.AddQdrant("vectordb");

var webApp = builder.AddProject<Projects.MyChatApp_Web>("aichatweb-app");
webApp.WithReference(openai);
webApp
    .WithReference(ingestionCache)
    .WaitFor(mcpServer)
    .WithReference(mcpServer)
    .WaitFor(storage)
    .WaitFor(blobs).WithReference(blobs)
    .WaitFor(queues).WithReference(queues)
    .WaitFor(qdrant).WithReference(qdrant)
    .WaitFor(ingestionCache);

var adminWeb = builder.AddProject<Projects.MyAdminApp_Web>("admin-web")
    .WaitFor(blobs).WithReference(blobs);

var functionApp = builder.AddAzureFunctionsProject<Projects.MyFunctionApp>("functions")
    .WaitFor(storage)
    .WithHostStorage(storage)
    .WaitFor(blobs).WithReference(blobs)
    .WaitFor(queues).WithReference(queues);

builder.Build().Run();
