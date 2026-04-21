using Dapper;
using Microsoft.Data.SqlClient;
using HelpDesk.Data;
using HelpDesk.Domain;
using HelpDesk.Endpoints;
using HelpDesk.Core;



var builder = WebApplication.CreateBuilder(args);

// OpenAPI
builder.Services.AddOpenApi();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;

    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// ---------- CORS ----------
var originsCsv = Environment.GetEnvironmentVariable("FRONTEND_ORIGINS")
    ?? builder.Configuration["Origins"]
    ?? builder.Configuration["AllowedOrigin"]
    ?? "http://localhost:5173,http://127.0.0.1:5173";

var allowedOrigins = originsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(o =>
{
    o.AddPolicy("Frontend", p => p
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

builder.Services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null;
    options.SerializerOptions.DictionaryKeyPolicy = null;
});
builder.Services.AddScoped<TicketCore>();
builder.Services.AddScoped<ThreadCore>();
builder.Services.AddScoped<UserCore>();

var app = builder.Build();

// Forwarded headers middleware SKAL være tidligt
app.UseForwardedHeaders();

// PRODUKTION: HSTS + HTTPS-redirect
if (app.Environment.IsProduction())
{
    app.UseHsts();            // sender Strict-Transport-Security header
    app.UseHttpsRedirection();
}

// OpenAPI UI + DEMO/DIAGNOSE: KUN i Development
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Demo/diagnose-eksempel
    var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };
    app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
            new WeatherForecast(
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20, 55),
                summaries[Random.Shared.Next(summaries.Length)]
            )).ToArray();
        return forecast;
    });

    // DB-ping (dev only)
    app.MapGet("/db-ping", async (ISqlConnectionFactory f) =>
    {
        await using var conn = f.Create() as SqlConnection;
        await conn!.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT 1";
        var result = (int)(await cmd.ExecuteScalarAsync() ?? 0);
        return Results.Json(new { ok = result == 1 });
    });
}


app.MapGet("/auth/debug", (HttpContext ctx) =>
{
    return Results.Ok(new
    {
        UserId = ctx.Session.GetInt32("UserId"),
        Role = ctx.Session.GetString("Role")
    });
});


// CORS
app.UseCors("Frontend");

// Session
app.UseSession();

// Health endpoints
app.MapGet("/health/ready", () => Results.Ok(new { ok = true }));
app.MapGet("/api/health/ready", () => Results.Ok(new { ok = true })).AllowAnonymous();

// API Endpoints
app.MapTicketEndpoints();
app.MapThreadCreationEndpoint();
app.MapLoginEndpoint();



app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
