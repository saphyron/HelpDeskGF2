using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HelpDesk.Domain;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace HelpDeskFrontend.Services;

public class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
        _http.BaseAddress = new Uri("http://localhost:5173"); // backend URL
    }

    
    public async Task<LoginResponse> LoginAsync(LoginRequest req)
    {
        var response = await _http.PostAsJsonAsync("/user/login", req);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        return await response.Content.ReadFromJsonAsync<LoginResponse>();
    }
    
    public async Task LoginAsGuestAsync()
    {
        await _http.PostAsync("/user/guest", null);
    }

    public async Task<List<TicketListItemDto>> GetTicketListAsync()
    => await _http.GetFromJsonAsync<List<TicketListItemDto>>("/tickets/list")
       ?? new();

    public async Task<List<TicketListItemDto>> GetArchivedTicketsAsync(DateOnly? date)
    {
        var url = date == null
            ? "/tickets/archived"
            : $"/tickets/archived?date={date:yyyy-MM-dd}";

        return await _http.GetFromJsonAsync<List<TicketListItemDto>>(url)
            ?? new();
    }

    
    public async Task CreateTicketAsync(int userid, string role)
    {
        var response = await _http.PostAsJsonAsync("/tickets", new {UserId = userid, Role = role});
        if (response.IsSuccessStatusCode) return;
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        throw new Exception(error?.Message ?? "Could not create ticket");
    }

    public class ErrorResponse
    {
        public string? Message { get; set; }
    }


    public async Task<LoginResponse?> Login(LoginRequest req)
        => await (await _http.PostAsJsonAsync("/user/login", req))
                            .Content.ReadFromJsonAsync<LoginResponse>();

    public async Task<(bool Success, List<ThreadSummary> Threads, string? Error)>
        GetThreadsSafe(int? userId, string role)
    {
        try
        {
            var url = $"/thread/threads?userId={userId}&role={role}";
            var response = await _http.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return (false, new List<ThreadSummary>(),
                    "Systemet er midlertidigt utilgængeligt (database-fejl).");
            }

            var data = await response.Content
                .ReadFromJsonAsync<List<ThreadSummary>>();

            return (true, data ?? new(), null);
        }
        catch (HttpRequestException)
        {
            return (false, new List<ThreadSummary>(),
                "Kan ikke oprette forbindelse til serveren.");
        }
    }

    public async Task<int> GetNumberTicketsForTheDay()
    {
            return await _http.GetFromJsonAsync<int>("/tickets/today/count");
    }

    public async Task<ThreadDto?> GetThread(int id)
        => await _http.GetFromJsonAsync<ThreadDto>($"/thread/threads/{id}");

    public async Task CreateThread(CreateThreadDto model)
    {
        var response = await _http.PostAsJsonAsync("/thread/threads", model);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            throw new Exception(error?.Message ?? "Could not create thread");
        }
    }

    public async Task AddResponse(int threadId, AddThreadResponseDto model)
        => await _http.PostAsJsonAsync($"/thread/threads/{threadId}/responses", model);
}
