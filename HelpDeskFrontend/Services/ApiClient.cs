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

    public async Task<LoginResponse?> Login(LoginRequest req)
        => await (await _http.PostAsJsonAsync("/user/login", req))
                            .Content.ReadFromJsonAsync<LoginResponse>();

    public async Task<List<ThreadSummary>> GetThreads()
        => await _http.GetFromJsonAsync<List<ThreadSummary>>("/thread/threads");

    public async Task<ThreadDto?> GetThread(int id)
        => await _http.GetFromJsonAsync<ThreadDto>($"/thread/threads/{id}");

    public async Task CreateThread(CreateThreadDto model)
        => await _http.PostAsJsonAsync("/thread/threads", model);

    public async Task AddResponse(int threadId, AddThreadResponseDto model)
        => await _http.PostAsJsonAsync($"/thread/threads/{threadId}/responses", model);
}
