using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HelpDesk.Domain;

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

    
    public async Task CreateTicketAsync()
    {
        var response = await _http.PostAsync("/tickets", null);

        if (!response.IsSuccessStatusCode)
            throw new Exception("Could not create ticket");
    }




    public async Task<LoginResponse?> Login(LoginRequest req)
        => await (await _http.PostAsJsonAsync("/user/login", req))
                            .Content.ReadFromJsonAsync<LoginResponse>();

    public async Task<(bool Success, List<ThreadSummary> Threads, string? Error)>
        GetThreadsSafe()
    {
        try
        {
            var response = await _http.GetAsync("/thread/threads");

            if (!response.IsSuccessStatusCode)
            {
                return (false, new List<ThreadSummary>(),
                    "Systemet er midlertidigt utilgængeligt (database-fejl).");
            }

            var data = await response.Content
                .ReadFromJsonAsync<List<ThreadSummary>>();

            return (true, data ?? new List<ThreadSummary>(), null);
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
        => await _http.PostAsJsonAsync("/thread/threads", model);

    public async Task AddResponse(int threadId, AddThreadResponseDto model)
        => await _http.PostAsJsonAsync($"/thread/threads/{threadId}/responses", model);
}
