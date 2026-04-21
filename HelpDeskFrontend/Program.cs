using HelpDeskFrontend.Services;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

builder.Services.AddHttpClient<ApiClient>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        UseCookies = true,
        AllowAutoRedirect = false
    });

var app = builder.Build();

app.UseStaticFiles();
app.UseSession();
app.MapRazorPages();

app.UseExceptionHandler("/Error");
app.UseHsts();

app.Run();