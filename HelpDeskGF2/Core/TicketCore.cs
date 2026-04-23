using HelpDesk.Domain;
using HelpDesk.Data;
using Dapper;

namespace HelpDesk.Core;

public class TicketCore
{
    private readonly ISqlConnectionFactory _factory;

    public TicketCore(ISqlConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<TicketDto> CreateTicket(int userId)
    {
        using var conn = _factory.Create();
        conn.Open();

        var hasActive = await conn.ExecuteScalarAsync<int>(
            @"select count(1)
              from dbo.Tickets
              where UserId = @UserId
                and CreatedAt >= cast(getdate() as date)
                and Status in ('Waiting', 'Open')",
            new { UserId = userId});

        if (hasActive > 0)
            throw new InvalidOperationException("User already has an active ticket");

        // position = antal ventende tickets før denne
        var position = await conn.ExecuteScalarAsync<int>(
            @"select count(1)
              from dbo.Tickets
              where CreatedAt >= cast(getdate() as date)
                and Status = 'Waiting'");
            

        var sql = @"
            insert into dbo.Tickets (UserId, Status, PositionInQueue)
            output inserted.TicketId
            values (@UserId, @Status, @PositionInQueue);";

        var id = await conn.ExecuteScalarAsync<int>(sql, new
        {
            UserId = userId,
            Status = TicketStatus.Waiting,
            PositionInQueue = position
        });

        return new TicketDto
        {
            TicketId = id,
            UserId = userId,
            CreatedAt = DateTime.Now,
            Status = TicketStatus.Waiting,
            PositionInQueue = position
        };
    }

    public async Task<IEnumerable<TicketDto>> GetTodayTickets()
    {
        using var conn = _factory.Create();
        return await conn.QueryAsync<TicketDto>(
            @"select TicketId, UserId, CreatedAt, Status
              from dbo.Tickets
              where CreatedAt >= cast(getdate() as date)
              order by Status, CreatedAt");
    }

    public async Task<int> GetNumberTicketsForTheDay()
    {
        using var conn = _factory.Create();
        return await conn.ExecuteScalarAsync<int>(
            @"select count(1)
              from dbo.Tickets
              where CreatedAt >= cast(getdate() as date)");
    }

    public async Task OpenNextTicket()
    {
        using var conn = _factory.Create();
        await conn.ExecuteAsync(
            @"update top (1) dbo.Tickets
              set Status = @Open
              where Status = @Waiting
              order by CreatedAt",
            new { Open = TicketStatus.Open, Waiting = TicketStatus.Waiting });
    }

    public async Task CloseDay()
    {
        using var conn = _factory.Create();
        await conn.ExecuteAsync(
            @"update dbo.Tickets
              set Status = @Archived
              where Status != @Archived",
            new { Archived = TicketStatus.Archived });
    }

    public async Task<bool> HasActiveTicket(int userId)
    {
        using var conn = _factory.Create();
        return await conn.ExecuteScalarAsync<int>(
            @"select count(1)
              from dbo.Tickets
              where UserId = @UserId
                and CreatedAt >= cast(getdate() as date)
                and Status in ('Waiting', 'Open')",
            new { UserId = userId}) > 0;
    }

    
    public async Task<IEnumerable<TicketListItemDto>> GetTicketList()
    {
        using var conn = _factory.Create();

        var sql = @"
            select 
                t.TicketId,
                u.Username,
                t.Status,
                t.PositionInQueue,
                t.CreatedAt
            from dbo.Tickets t
            join dbo.Users u on u.UserId = t.UserId
            where t.Status in ('Open', 'Waiting')
            order by
                case
                    when t.Status = 'Open' then 0
                    when t.Status = 'Waiting' then 1
                    else 2
                end,
                t.CreatedAt asc;
        ";

        return await conn.QueryAsync<TicketListItemDto>(sql);
    }

    public async Task<IEnumerable<TicketListItemDto>> GetArchivedTickets(DateOnly? date = null)
    {
        using var conn = _factory.Create();

        var sql = @"
            select 
                t.TicketId,
                u.Username,
                t.Status,
                t.PositionInQueue,
                t.CreatedAt
            from dbo.Tickets t
            join dbo.Users u on u.UserId = t.UserId
            where t.Status = 'Archived'
        ";

        if (date != null)
            sql += " and cast(t.CreatedAt as date) = @Date";

        sql += " order by t.CreatedAt desc;";

        return await conn.QueryAsync<TicketListItemDto>(
            sql,
            new { Date = date?.ToDateTime(TimeOnly.MinValue) }
        );
    }

}