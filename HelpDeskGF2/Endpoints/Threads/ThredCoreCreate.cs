using Dapper;
using Microsoft.AspNetCore.Http.HttpResults;
using HelpDesk.Data;
using HelpDesk.Domain;

namespace HelpDesk.Endpoints;

public static class ThreadCreation
{
    public static RouteGroupBuilder MapThreadCreationEndpoint(this IEndpointRouteBuilder g)
    {
        var group = g.MapGroup("/thread/");
        /*
        PUT /thread/threads/{id}
        POST /thread/threads/{id}/responses
        */

        group.MapGet("/threads", async (SqlConnectionFactory factory) =>
        {
            using var conn = factory.Create();
            conn.Open();
            var sql = @"select *
                        from dbo.Threads
                        order by case status 
                            when 'working on' then 1
                            when 'open' then 2
                            when 'closed' then 3
                            else 4 end, CreatedAt desc";
            var threads = await conn.QueryAsync<ThreadSummary>(sql);
            return Results.Ok(threads);
        }).AllowAnonymous();

        group.MapGet("/threads/{id:int}", async (int id, SqlConnectionFactory factory) =>
        {
            using var conn = factory.Create();
            conn.Open();
            var sql = @"select *
                        from dbo.Threads
                        where Id = @Id;
                        select *
                        from dbo.ThreadResponses r
                        where r.ThreadId = @Id";
            using var multi = await conn.QueryMultipleAsync(sql, new { Id = id });
            var thread = await multi.ReadSingleOrDefaultAsync<ThreadDto>();
            if (thread is null)
                return Results.NotFound();
            thread.Responses = (await multi.ReadAsync<ThreadResponseDto>()).ToList();
            return Results.Ok(thread);
        }).AllowAnonymous();
        
        group.MapPost("/threads", async Task<Results<Ok, BadRequest<string>>> (
            CreateThreadDto body,
            SqlConnectionFactory factory) =>
        {
            if (string.IsNullOrWhiteSpace(body.Title))
                return TypedResults.BadRequest("Title is required");
            if (string.IsNullOrWhiteSpace(body.ThreadBody))
                body.ThreadBody = "";

            using var conn = factory.Create();
            conn.Open();
            var sql = @"insert into dbo.Threads (Title, CreatedByUserId, ThreadBody)
                        values (@Title, @CreatedByUserId, @ThreadBody);";
            var rows = await conn.ExecuteAsync(sql, body);
            if (rows == 1)
                return TypedResults.Ok();
            return TypedResults.BadRequest("Failed to create thread");
        }).AllowAnonymous();

        group.MapPut("/threads/{id:int}", async Task<Results<Ok, NotFound, BadRequest<string>>> (
            int id,
            UpdateThreadDto body,
            SqlConnectionFactory factory) =>
        {
            using var conn = factory.Create();
            conn.Open();
            var existing = await conn.QuerySingleOrDefaultAsync<ThreadDto>(
                "select * from dbo.Threads where Id = @Id", new { Id = id });
            if (existing is null)
                return TypedResults.NotFound();
            var title = body.Title ?? existing.Title;
            var threadBody = body.ThreadBody ?? existing.ThreadBody;
            var sql = @"update dbo.Threads
                        set Title = @Title, ThreadBody = @ThreadBody
                        where Id = @Id";
            var rows = await conn.ExecuteAsync(sql, new { Id = id, Title = title, ThreadBody = threadBody });
            if (rows == 1)
                return TypedResults.Ok();
            return TypedResults.BadRequest("Failed to update thread");
        }).AllowAnonymous();

        group.MapPost("/threads/{id:int}/responses", async Task<Results<Ok, NotFound, BadRequest<string>>> (
            int id,
            AddThreadResponseDto body,
            SqlConnectionFactory factory) =>
        {
            using var conn = factory.Create();
            conn.Open();
            var existing = await conn.QuerySingleOrDefaultAsync<ThreadDto>(
                "select * from dbo.Threads where Id = @Id", new { Id = id });
            if (existing is null)
                return TypedResults.NotFound();
            if (string.IsNullOrWhiteSpace(body.ResponseBody))
                return TypedResults.BadRequest("ResponseBody is required");
            var sql = @"insert into dbo.ThreadResponses (ThreadId, ResponseBody, CreatedByUserId)
                        values (@ThreadId, @ResponseBody, @CreatedByUserId);";
            var rows = await conn.ExecuteAsync(sql, new
            {
                ThreadId = id,
                body.ResponseBody,
                body.CreatedByUserId
            });
            if (rows == 1)
                return TypedResults.Ok();
            return TypedResults.BadRequest("Failed to add response");
        }).AllowAnonymous();

        return group;
    }
}