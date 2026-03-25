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
        // Get all threads
        group.MapGet("/threads", async (ISqlConnectionFactory factory) =>
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
        // Get threads by id
        group.MapGet("/threads/{id:int}", async (int id, ISqlConnectionFactory factory) =>
        {
            using var conn = factory.Create();
            conn.Open();
            var sql = @"select *
                        from dbo.Threads
                        where ThreadId = @Id;
                        select *
                        from dbo.ThreadResponses r
                        where r.ThreadId = @Id";
            using var multi = await conn.QueryMultipleAsync(sql, new { ThreadId = id });
            var thread = await multi.ReadSingleOrDefaultAsync<ThreadDto>();
            if (thread is null)
                return Results.NotFound();
            thread.Responses = (await multi.ReadAsync<ThreadResponseDto>()).ToList();
            return Results.Ok(thread);
        }).AllowAnonymous();
       // Create thread 
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
        // Update thread
        group.MapPut("/threads/{id:int}", async Task<Results<Ok, NotFound, BadRequest<string>>> (
            int id,
            UpdateThreadDto body,
            ISqlConnectionFactory factory) =>
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
                        where ThreadId = @Id";
            var rows = await conn.ExecuteAsync(sql, new { ThreadId = id, Title = title, ThreadBody = threadBody });
            if (rows == 1)
                return TypedResults.Ok();
            return TypedResults.BadRequest("Failed to update thread");
        }).AllowAnonymous();
        // Add response to thread
        group.MapPost("/threads/{id:int}/responses", async Task<Results<Ok, NotFound, BadRequest<string>>> (
            int id,
            AddThreadResponseDto body,
            ISqlConnectionFactory factory) =>
        {
            using var conn = factory.Create();
            conn.Open();
            var existing = await conn.QuerySingleOrDefaultAsync<ThreadDto>(
                "select * from dbo.Threads where ThreadId = @Id", new { Id = id });
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
        // Update ResponseBody
        group.MapPut("/threads/{threadId:int}/responses/{responseId:int}", async Task<Results<Ok, NotFound, BadRequest<string>>> (
            int threadId,
            int responseId,
            string responseBody,
            ISqlConnectionFactory factory) =>
        {
            using var conn = factory.Create();
            conn.Open();
            var existing = await conn.QuerySingleOrDefaultAsync<ThreadResponseDto>(
                "select * from dbo.ThreadResponses where ResponseId = @ResponseId and ThreadId = @ThreadId",
                new { ResponseId = responseId, ThreadId = threadId });
            if (existing is null)
                return TypedResults.NotFound();
            if (string.IsNullOrWhiteSpace(responseBody))
                return TypedResults.BadRequest("ResponseBody is required");
            var sql = @"update dbo.ThreadResponses
                        set ResponseBody = @ResponseBody
                        where ResponseId = @ResponseId and ThreadId = @ThreadId";
            var rows = await conn.ExecuteAsync(sql, new
            {
                ResponseId = responseId,
                ThreadId = threadId,
                ResponseBody = responseBody
            });
            if (rows == 1)
                return TypedResults.Ok();
            return TypedResults.BadRequest("Failed to update response");
        }).AllowAnonymous();
        // List endpoints with role-based filtering
        group.MapGet("/Threads/{role:string}&{id:int}/list", async (string role, int id, ISqlConnectionFactory factory) =>
        {
            using var conn = factory.Create();
            conn.Open();
            string sql;
            // User
            if (role == "user")
            {
                sql = @"select *
                        from dbo.Threads
                        where CreatedByUserId = @UserId
                        order by case status 
                            when 'working on' then 1
                            when 'open' then 2
                            when 'closed' then 3
                            else 4 end, CreatedAt desc";
            }
            // Guest
            else if (role == "guest")
            {
                sql = @"select *
                        from dbo.Threads
                        where CreatedByUserId = @UserId
                        order by case status 
                            when 'working on' then 1
                            when 'open' then 2
                            when 'closed' then 3
                            else 4 end, CreatedAt desc";
            }
            // Invalid role
            else
            {
                return Results.BadRequest("Invalid role");
            }
            var threads = await conn.QueryAsync<ThreadSummary>(sql, new { UserId = id });
            return Results.Ok(threads);
        }).AllowAnonymous();
        // Admin - all threads
        group.MapGet("/Threads/admin&{status:string}/list", async (string status, ISqlConnectionFactory factory) =>
        {
            using var conn = factory.Create();
            conn.Open();
            string sql;
            // Admin
            if (status == "all")
            {
                sql = @"select *
                        from dbo.Threads
                        order by case status 
                            when 'working on' then 1
                            when 'open' then 2
                            when 'closed' then 3
                            else 4 end, CreatedAt desc";
            }
            if (status == "open")
            {
                sql = @"select *
                        from dbo.Threads
                        where status != 'closed'
                        order by case status 
                            when 'working on' then 1
                            when 'open' then 2
                            when 'closed' then 3
                            else 4 end, CreatedAt desc";
            }
            else
            {
                return Results.BadRequest("Invalid role");
            }
            var threads = await conn.QueryAsync<ThreadSummary>(sql);
            return Results.Ok(threads);
        }).AllowAnonymous();
        return group;
    }
}