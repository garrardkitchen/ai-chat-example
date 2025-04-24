var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//builder.Services.AddControllersWithViews();
//builder.Services.AddRazorPages();

// Register configuration for dependency injection
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<GitLabTools>()
    .WithTools<WhoIsTool>();

var app = builder.Build();

app.MapMcp();

await app.RunAsync();