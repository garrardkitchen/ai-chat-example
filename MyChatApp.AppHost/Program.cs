var builder = DistributedApplication.CreateBuilder(args);

// You will need to set the connection string to your own value
// You can do this using Visual Studio's "Manage User Secrets" UI, or on the command line:
//   cd this-project-directory
//   dotnet user-secrets set ConnectionStrings:openai "Endpoint=https://models.inference.ai.azure.com;Key=YOUR-API-KEY"
var openai = builder.AddConnectionString("openai");

var ingestionCache = builder.AddSqlite("ingestionCache");

// Add Azure Blob Storage for file uploads
var blobStorage = builder.AddAzureStorage("blobStorage").RunAsEmulator().AddBlobs("blobs");

var mcpServer = builder.AddProject<Projects.MyMcpServerHttpApi>("mcp-server").WithExternalHttpEndpoints();

var webApp = builder.AddProject<Projects.MyChatApp_Web>("aichatweb-app");
webApp.WithReference(openai);
webApp
    .WithReference(ingestionCache)
    .WaitFor(mcpServer)
    .WithReference(mcpServer)
    .WaitFor(blobStorage)
    .WithReference(blobStorage)
    .WaitFor(ingestionCache);

var adminWeb = builder.AddProject<Projects.MyAdminApp_Web>("admin-web")
    .WaitFor(blobStorage)
    .WithReference(blobStorage);
    

builder.Build().Run();
