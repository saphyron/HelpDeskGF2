using Dapper;
using HelpDesk.Data;
using HelpDesk.Domain;

namespace HelpDesk.Core;

public class ThreadCore
{
    private readonly ISqlConnectionFactory _factory;

    public ThreadCore(ISqlConnectionFactory factory)
    {
        _factory = factory;
    }

    /* ---------- THREADS ---------- */

    public async Task<IEnumerable<ThreadSummary>> GetAllThreads()
    {
        using var conn = _factory.Create();
        conn.Open();

        var sql = """
            select *
            from dbo.Threads
            order by case status 
                when 'working on' then 1
                when 'open' then 2
                when 'closed' then 3
                else 4 end,
                CreatedAt desc
        """;

        return await conn.QueryAsync<ThreadSummary>(sql);
    }

    public async Task<ThreadDto?> GetThreadById(int id)
    {
        using var conn = _factory.Create();
        conn.Open();

        var sql = """
            select *
            from dbo.Threads
            where ThreadId = @Id;

            select *
            from dbo.ThreadResponses
            where ThreadId = @Id;
        """;

        using var multi = await conn.QueryMultipleAsync(sql, new { Id = id });

        var thread = await multi.ReadSingleOrDefaultAsync<ThreadDto>();
        if (thread is null)
            return null;

        thread.Responses = (await multi.ReadAsync<ThreadResponseDto>()).ToList();
        return thread;
    }

    public async Task<bool> CreateThread(CreateThreadDto dto)
    {
        using var conn = _factory.Create();
        conn.Open();

        var sql = """
            insert into dbo.Threads
                (Title, ThreadBody, CreatedByUserId, AnonymousName)
            values
                (@Title, @ThreadBody, @CreatedByUserId, @AnonymousName);
        """;

        var rows = await conn.ExecuteAsync(sql, dto);
        return rows == 1;
    }

    public async Task<bool> UpdateThread(int id, UpdateThreadDto dto)
    {
        using var conn = _factory.Create();
        conn.Open();

        var existing = await conn.QuerySingleOrDefaultAsync<ThreadDto>(
            "select * from dbo.Threads where ThreadId = @Id",
            new { Id = id });

        if (existing is null)
            return false;

        var sql = """
            update dbo.Threads
            set Title = @Title,
                ThreadBody = @ThreadBody
            where ThreadId = @ThreadId
        """;

        var rows = await conn.ExecuteAsync(sql, new
        {
            ThreadId = id,
            Title = dto.Title ?? existing.Title,
            ThreadBody = dto.ThreadBody ?? existing.ThreadBody
        });

        return rows == 1;
    }

    /* ---------- RESPONSES ---------- */

    public async Task<bool> AddResponse(int threadId, AddThreadResponseDto dto)
    {
        using var conn = _factory.Create();
        conn.Open();

        var exists = await conn.ExecuteScalarAsync<int>(
            "select count(1) from dbo.Threads where ThreadId = @Id",
            new { Id = threadId });

        if (exists == 0)
            return false;

        var sql = """
            insert into dbo.ThreadResponses
                (ThreadId, ResponseBody, CreatedByUserId, AnonymousName)
            values
                (@ThreadId, @ResponseBody, @CreatedByUserId, @AnonymousName);
        """;

        var rows = await conn.ExecuteAsync(sql, new
        {
            ThreadId = threadId,
            dto.ResponseBody,
            dto.CreatedByUserId,
            dto.AnonymousName
        });

        return rows == 1;
    }

    public async Task<bool> UpdateResponse(
        int threadId,
        int responseId,
        string responseBody)
    {
        using var conn = _factory.Create();
        conn.Open();

        var exists = await conn.QuerySingleOrDefaultAsync<ThreadResponseDto>(
            """
            select *
            from dbo.ThreadResponses
            where ResponseId = @ResponseId
              and ThreadId = @ThreadId
            """,
            new { ResponseId = responseId, ThreadId = threadId });

        if (exists is null)
            return false;

        var sql = """
            update dbo.ThreadResponses
            set ResponseBody = @ResponseBody
            where ResponseId = @ResponseId
              and ThreadId = @ThreadId
        """;

        var rows = await conn.ExecuteAsync(sql, new
        {
            ResponseId = responseId,
            ThreadId = threadId,
            ResponseBody = responseBody
        });

        return rows == 1;
    }
}
