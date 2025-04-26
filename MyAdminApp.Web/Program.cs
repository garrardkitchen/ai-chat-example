using Azure.Identity;
using Aspire.Azure.Storage.Blobs;
using MyAdminApp.Web.Components;
using MyAdminApp.Web.Services;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.AddAzureBlobClient("blobs");
//
// // Add Azure Blob Storage client
// var blobStorageConnectionString = builder.Configuration.GetConnectionString("blobStorage");
// if (!string.IsNullOrEmpty(blobStorageConnectionString))
// {
//     // Use connection string if available (local dev)
//     builder.Services.AddSingleton(sp => new BlobServiceClient(blobStorageConnectionString));
// }
// else
// {
//     // Otherwise, use Azure Identity with DefaultAzureCredential for authentication in Azure
//     var blobStorageUri = builder.Configuration["BlobStorage:ServiceUri"];
//     if (!string.IsNullOrEmpty(blobStorageUri))
//     {
//         builder.Services.AddSingleton(sp => new BlobServiceClient(
//             new Uri(blobStorageUri), 
//             new DefaultAzureCredential()));
//     }
// }

// Register BlobStorageService
builder.Services.AddScoped<BlobStorageService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
