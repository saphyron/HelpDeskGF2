using HelpDeskFrontend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

// HttpClient → backend
builder.Services.AddHttpClient<ApiClient>();

var app = builder.Build();

app.UseSession();
app.MapRazorPages();

app.Run();