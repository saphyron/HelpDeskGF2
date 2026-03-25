using Dapper;
using Microsoft.Data.SqlClient;
using HelpDesk.Data;
using HelpDesk.Domain;
using HelpDesk.Endpoints;



var builder = WebApplication.CreateBuilder(args);

// OpenAPI
builder.Services.AddOpenApi();

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

// ---------- Connection string ----------
string? connFromSettings = builder.Configuration.GetConnectionString("Default");

var sqlServer   = builder.Configuration["SQL_SERVER"];
var sqlDb       = builder.Configuration["SQL_DATABASE"];
var sqlUser     = builder.Configuration["SQL_USER"];
var sqlPass     = builder.Configuration["SQL_PASSWORD"];
var sqlEncrypt  = builder.Configuration["SQL_ENCRYPT"];
var sqlTsc      = builder.Configuration["SQL_TRUST_SERVER_CERTIFICATE"];
var sqlTrustCon = builder.Configuration["SQL_TRUST_CONNECTION"];

string connString;
if (!string.IsNullOrWhiteSpace(sqlServer) &&
    !string.IsNullOrWhiteSpace(sqlDb) &&
    !string.IsNullOrWhiteSpace(sqlUser))
{
    var sb = new SqlConnectionStringBuilder
    {
        DataSource = sqlServer,
        InitialCatalog = sqlDb,
        UserID = sqlUser,
        Password = sqlPass,
        Encrypt = string.Equals(sqlEncrypt, "true", StringComparison.OrdinalIgnoreCase),
        TrustServerCertificate = string.Equals(sqlTsc ?? "true", "true", StringComparison.OrdinalIgnoreCase),
    };
    connString = sb.ConnectionString;
}
else
{
    connString = connFromSettings ?? throw new InvalidOperationException("No connection string configured.");
}

builder.Services.AddSingleton(new SqlConnectionFactory(connString));
builder.Services.AddScoped<Db>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null;
    options.SerializerOptions.DictionaryKeyPolicy = null;
});

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
    app.MapGet("/db-ping", async (SqlConnectionFactory f) =>
    {
        await using var conn = f.Create() as SqlConnection;
        await conn!.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT 1";
        var result = (int)(await cmd.ExecuteScalarAsync() ?? 0);
        return Results.Json(new { ok = result == 1 });
    });
}

// CORS
app.UseCors("Frontend");

// Health endpoints
app.MapGet("/health/ready", () => Results.Ok(new { ok = true }));
app.MapGet("/api/health/ready", () => Results.Ok(new { ok = true })).AllowAnonymous();

// API Endpoints


app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
